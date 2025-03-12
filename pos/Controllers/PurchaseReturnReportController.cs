 using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pos.Database;

namespace pos.Controllers
{
    public class PurchaseReturnReportController : Controller
    {
        protected readonly AppDbContext _context;
        public PurchaseReturnReportController(AppDbContext context)
        {
            _context = context;
        }

        // Get Index
        public IActionResult Index()
        {
            return View();
        }

        // Get PurchaseReturnReport
        public async Task<IActionResult> GetPurchaseReturnReport()
        {
            var purhaseReturned = await _context.ItemReturneds
                .Include(i => i.IncomingItem)
                    .ThenInclude(i => i.Item)
                .OrderByDescending(i => i.Id)
                .Select(i => new
                {
                    i.Id,
                    itemName = i.IncomingItem.Item.Name,
                    batchNumber = i.IncomingItem.BatchNumber,
                    stockReturned = i.StockReturned,
                    information = i.Information
                })
                .ToListAsync();
            return Json(new { data = purhaseReturned });
        }

        // Print PurchaseReturnReport
        public async Task<IActionResult> PrintPurchaseReturnReport()
        {
            var purhaseReturned = await _context.ItemReturneds
                .Include(i => i.IncomingItem)
                    .ThenInclude(i => i.Item)
                    .OrderByDescending(i => i.Id)
                    .Select(i => new
                    {
                        i.Id,
                        itemName = i.IncomingItem.Item.Name,
                        batchNumber = i.IncomingItem.BatchNumber,
                        stockReturned = i.StockReturned,
                        information = i.Information
                    })
                .ToListAsync();

            var htmlContent = @"
                <h1 style='text-align:center; font-family:Arial, sans-serif;'>Purchase Returned Report</h1>
                <table style='width: 100%; border-collapse: collapse; text-align: left; font-family: Arial, sans-serif;'>
                    <thead style='background-color: #f2f2f2;'>
                        <tr>
                            <th style='padding: 8px; border: 1px solid #ddd;'>No</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Item Name</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Stock Returned</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Information</th>
                        </tr>
                    </thead>
                    <tbody>";

            for (int i = 0; i < purhaseReturned.Count; i++)
            {
                var item = purhaseReturned[i];
                htmlContent += $@"
                                     <tr style='border-bottom: 1px solid #ddd;'>
                                                <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{i + 1}</td>
                                                <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{item.itemName}</td>
                                                <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{item.stockReturned}</td>
                                                <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{item.information}</td>
                                            </tr>"
                ;
            }

            htmlContent += @"
                    </tbody>
                </table>";

            var pdfConverter = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    Out = null
                }
            };

            pdfConverter.Objects.Add(new ObjectSettings
            {
                HtmlContent = htmlContent,
                WebSettings = new WebSettings
                {
                    DefaultEncoding = "utf-8",
                }
            });

            var converter = HttpContext.RequestServices.GetRequiredService<IConverter>();
            var pdf = converter.Convert(pdfConverter);

            return File(pdf, "application/pdf", "Item_Stock_Report.pdf");
        }
    }
}
