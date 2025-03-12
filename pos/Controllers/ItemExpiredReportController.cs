using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pos.Database;

namespace pos.Controllers
{
    public class ItemExpiredReportController : Controller
    {
        protected readonly AppDbContext _context;
        public ItemExpiredReportController(AppDbContext context)
        {
            _context = context;
        }

        // Get Index
        public IActionResult Index()
        {
            return View();
        }

        // Get ItemExpired
        public async Task<IActionResult> GetItemExpired()
        {
            //Check if the item is expired
            DateOnly threeMonthsLater = DateOnly.FromDateTime(DateTime.Now.AddMonths(3));
            var ItemExpired = await _context.IncomingItems
                .Include(i => i.Item)
                .Include(i => i.Supplier)
                .OrderByDescending(i => i.Id)
                .Where(i => i.ExpiredDate <= threeMonthsLater)
                .Select(i => new
                {
                    batchNumber = i.BatchNumber,
                    itemName = i.Item.Name,
                    stockIn = i.StockIn,
                    expiredDate = i.ExpiredDate.ToString("yyyy-MM-dd"),
                    supplierName = i.Supplier.Name
                })
                .ToListAsync();

            return Json(new { data = ItemExpired });
        }

        // Print ItemExpired
        public async Task<IActionResult> PrintItemExpired()
        {
            //Check if the item is expired
            DateOnly threeMonthsLater = DateOnly.FromDateTime(DateTime.Now.AddMonths(3));
            var ItemExpired = await _context.IncomingItems
                .Include(i => i.Item)
                .Include(i => i.Supplier)
                .OrderByDescending(i => i.Id)
                .Where(i => i.ExpiredDate <= threeMonthsLater)
                .Select(i => new
                {
                    batchNumber = i.BatchNumber,
                    itemName = i.Item.Name,
                    stockIn = i.StockIn,
                    expiredDate = i.ExpiredDate.ToString("yyyy-MM-dd"),
                    supplierName = i.Supplier.Name
                })
                .ToListAsync();
            var htmlContent = @"
                <h1 style='text-align:center; font-family:Arial, sans-serif;'>Item Expired Report</h1>
                <table style='width: 100%; border-collapse: collapse; text-align: left; font-family: Arial, sans-serif;'>
                    <thead style='background-color: #f2f2f2;'>
                        <tr>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Batch Number</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Item Name</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Stock Expired</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Expired Date</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Supplier Name</th>
                        </tr>
                    </thead>
                    <tbody>";
            for (int i = 0; i < ItemExpired.Count; i++)
            {
                var item = ItemExpired[i];
                htmlContent += $@"
                    <tr>
                        <td style='padding: 8px; border: 1px solid #ddd;'>{item.batchNumber}</td>
                        <td style='padding: 8px; border: 1px solid #ddd;'>{item.itemName}</td>
                        <td style='padding: 8px; border: 1px solid #ddd;'>{item.stockIn}</td>
                        <td style='padding: 8px; border: 1px solid #ddd;'>{item.expiredDate}</td>
                        <td style='padding: 8px; border: 1px solid #ddd;'>{item.supplierName}</td>
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

            return File(pdf, "application/pdf", "Item_Expired_Report.pdf");
        }

    }

}
