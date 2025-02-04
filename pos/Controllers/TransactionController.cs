using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pos.Database;
using pos.Models;
using DinkToPdf;
using System.Text;


namespace pos.Controllers
{
    [ApiController]
    [Route("Transaction")]
    public class TransactionController : Controller
    {
        protected readonly AppDbContext _context;
        private readonly IConverter _pdfConverter;

        public TransactionController(AppDbContext context, IConverter converter)
        {
            _context = context;
            _pdfConverter = converter;
        }

        public async Task<IActionResult> Index()
        {
            //check expired date
            DateOnly threeMonthsLater = DateOnly.FromDateTime(DateTime.Now.AddMonths(3));
            var itemList = await _context.IncomingItems
                .Include(i => i.Item)
                .Include(i => i.Supplier)
                .Where(i => i.ExpiredDate >= threeMonthsLater)
                .Where(i => i.StockIn > 0)
                .OrderBy(i => i.DateOfEntry)
                .ThenBy(i => i.ExpiredDate)
                .ToListAsync();

            ViewBag.Customers = new SelectList(_context.Customers.ToList(), "Id", "Name");

            return View(itemList);
        }

        public class CheckoutRequest
        {
            public int CustomerId { get; set; }
            public string TotalAmount { get; set; }
            public string PaymentMethod { get; set; }
            public List<CartItem> CartItems { get; set; }
        }

        public class CartItem
        {
            public int TransactionId { get; set; }
            public string ItemName { get; set; }
            public int Quantity { get; set; }

            public decimal UnitPrice { get; set; }
            public decimal SubTotal { get; set; }
        }

        [HttpPost]
        [Route("Checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            using var transactionScope = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create transaction
                var transaction = new Transaction
                {
                    CustomerId = request.CustomerId,
                    TransactionCode = $"INV-{new Random().Next(10000, 99999)}",
                    TotalAmount = decimal.Parse(request.TotalAmount),
                    PaymentMethod = (PaymentMethod)Enum.Parse(typeof(PaymentMethod), request.PaymentMethod),
                    TransactionDate = DateTime.Now,
                    Status = TransactionStatus.Completed,
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                var transactionDetails = new List<TransactionDetail>();

                // Update stock in IncomingItems
                foreach (var item in request.CartItems)
                {
                    var incomingItem = await _context.IncomingItems
                        .Where(i => i.Item.Name == item.ItemName)
                        .OrderBy(i => i.DateOfEntry)
                        .ThenBy(i => i.ExpiredDate)
                        .FirstOrDefaultAsync();
                    if (incomingItem != null && incomingItem.StockIn >= item.Quantity)
                    {
                        incomingItem.StockIn -= item.Quantity;
                        _context.IncomingItems.Update(incomingItem);
                    }
                    else
                    {
                        throw new Exception($"Something when wrong");
                    }

                    var transactionDetail = new TransactionDetail
                    {
                        TransactionId = transaction.Id,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.SubTotal,
                    };
                    transactionDetails.Add(transactionDetail);
                }

                // Update stock in Items
                foreach (var item in request.CartItems)
                {
                    var itemDb = await _context.Items
                        .Where(i => i.Name == item.ItemName)
                        .FirstOrDefaultAsync();
                    if (itemDb != null && itemDb.stock >= item.Quantity)
                    {
                        itemDb.stock -= item.Quantity;
                        _context.Items.Update(itemDb);
                    }
                    else
                    {
                        throw new Exception($"Something when wrong");
                    }
                }

                _context.TransactionDetails.AddRange(transactionDetails);
                await _context.SaveChangesAsync();

                // Commit transaction when success
                await transactionScope.CommitAsync();

                return Json(new { success = true, transactionId = transaction.Id });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " Inner exception: " + ex.InnerException.Message;
                }
                return Json(new { success = false, message = errorMessage });
            }

        }

        [HttpGet("GenerateReceipt")]
        public async Task<IActionResult> GenerateReceipt([FromQuery] int transactionId, [FromQuery] decimal cashMoney, [FromQuery] decimal changeMoney)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Customer)
                .Include(t => t.TransactionDetails)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null)
            {
                return Json(new { success = false, message = "Transaction not found" });
            }

            string receiptHtml = GenerateReceiptHtml(transaction, cashMoney, changeMoney);
            var pdfDocument = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait
                },
                Objects = { new ObjectSettings { HtmlContent = receiptHtml } }
            };

            byte[] pdfBytes = _pdfConverter.Convert(pdfDocument);

            return File(pdfBytes, "application/pdf", $"Receipt_{transaction.TransactionCode}.pdf");

        }

        [HttpGet("GenerateReceipt")]
        private string GenerateReceiptHtml(Transaction transaction, decimal cashMoney, decimal changeMoney)
        {
            var sb = new StringBuilder();
            sb.Append(@"
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; text-align: center; }
                    .receipt-container { width: 300px; margin: auto; text-align: center; }
                    .receipt-header { font-size: 18px; font-weight: bold; }
                    .receipt-subheader { font-size: 12px; margin-bottom: 10px; }
                    .line { border-top: 1px dashed black; margin: 10px 0; }
                    .receipt-table { width: 100%; border-collapse: collapse; margin-top: 10px; }
                    .receipt-table td { padding: 5px; text-align: left; }
                    .total { font-weight: bold; }
                    .item-number { border: 2px solid green; padding: 2px 5px; font-weight: bold; display: inline-block; width: 20px; text-align: center; }
                    .text-right { text-align: right; }
                </style>
            </head>
            <body>
                <div class='receipt-container'>
                    <div class='receipt-header'>Point Of Sale Pharmacy</div>
                    <div class='receipt-subheader'>
                        Jl. Mangkuyudan, Rw.1, Karangmulyo, Kec. Purwodadi, Kabupaten Purworejo, Jawa Tengah 54173 <br>
                        No. Telp 081229248179 <br>
                        " + transaction.TransactionCode + @"
                    </div>

                    <div class='line'></div>

                    <div class='receipt-details'>
                        <table class='receipt-table'>
                            <tr>
                                <td><strong>" + transaction.TransactionDate.ToString("dd/MM/yyyy") + @"</strong></td>
                            </tr>
                            <tr>
                                <td>" + transaction.TransactionDate.ToString("HH:mm:ss") + @"</td>
                                <td class='text-right'>" + transaction.Customer.Name + @"</td>
                            </tr>
                        </table>
                    </div>

                    <div class='line'></div>

                    <table class='receipt-table'>");

                        int counter = 1;
                        foreach (var detail in transaction.TransactionDetails)
                        {
                            sb.Append(@"
                        <tr>
                            <td><span class='item-number'>" + counter + @"</span> <strong>" + detail.ItemName + @"</strong></td>
                            <td class='text-right'>Rp " + detail.UnitPrice.ToString("N0") + @"</td>
                        </tr>
                        <tr>
                            <td>" + detail.Quantity + " x " + detail.UnitPrice.ToString("N0") + @"</td>
                            <td class='text-right'>Rp " + detail.Subtotal.ToString("N0") + @"</td>
                        </tr>");
                            counter++;
                        }

                        sb.Append(@"

                        <tr class='total'>
                            <td><strong>Grand Total</strong></td>
                            <td class='text-right'><strong>Rp " + transaction.TotalAmount.ToString("N0") + @"</strong></td>
                        </tr>
                        <tr>
                            <td>Cash Money</td>
                            <td class='text-right'>Rp " + cashMoney.ToString("N0") + @"</td>
                        </tr>
                        <tr>
                            <td>Change Money</td>
                            <td class='text-right'>Rp " + changeMoney.ToString("N0") + @"</td>
                        </tr>
                    </table>

                    <div class='line'></div>

                    <p>Terimakasih Telah Berbelanja</p>
                </div>
            </body>
            </html>");

            return sb.ToString();
        }
    }
}
