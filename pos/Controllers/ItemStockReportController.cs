using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pos.Database;
using pos.Models;

namespace pos.Controllers
{
    public class ItemStockReportController : Controller
    {
        protected readonly AppDbContext _context;
        public ItemStockReportController(AppDbContext context)
        {
            _context = context;
        }

        // Get Index
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // Get ItemStockReport
        public async Task<IActionResult> GetItemStockReport()
        {
            var ItemStockReport = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Unit)
                .ToListAsync();
            
            return Json(new {data = ItemStockReport});
        }

        // Print ItemStockReport
        public async Task<IActionResult> PrintItemStockReport()
        {
            var ItemStockReport = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Unit)
                .ToListAsync();

            var htmlContent = @"
                <h1 style='text-align:center; font-family:Arial, sans-serif;'>Item Stock Report</h1>
                <table style='width: 100%; border-collapse: collapse; text-align: left; font-family: Arial, sans-serif;'>
                    <thead style='background-color: #f2f2f2;'>
                        <tr>
                            <th style='padding: 8px; border: 1px solid #ddd;'>No</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Code</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Item Name</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Current Stock</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Category</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Unit</th>
                        </tr>
                    </thead>
                    <tbody>";

                            for (int i = 0; i < ItemStockReport.Count; i++)
                            {
                                var item = ItemStockReport[i];
                                htmlContent += $@"
                                     <tr style='border-bottom: 1px solid #ddd;'>
                                                <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{i + 1}</td>
                                                <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{item.ItemCode}</td>
                                                <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{item.Name}</td>
                                                <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{item.stock}</td>
                                                <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{item.Category.Name}</td>
                                                <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{item.Unit.Name}</td>
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
