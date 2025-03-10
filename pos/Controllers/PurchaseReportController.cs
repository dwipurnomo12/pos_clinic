using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pos.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Text;

namespace pos.Controllers
{
    public class PurchaseReportController : Controller
    {
        protected readonly AppDbContext _context;
        public PurchaseReportController(AppDbContext context)
        {
            _context = context;
        }

        // Get Purchase Report
        public async Task<IActionResult> GetPurchaseReport([FromQuery] DateTime? start_date, [FromQuery] DateTime? end_date)
        {
            if (!start_date.HasValue || !end_date.HasValue)
            {
                return Json(new { message = "Select date range is required", data = new List<object>() });
            }

            var query = _context.IncomingItems
                .Include(i => i.Item)
                .Include(i => i.Supplier)
                .AsQueryable();

            if (start_date.HasValue)
            {
                query = query.Where(i => i.DateOfEntry >= start_date.Value);
            }

            if (end_date.HasValue)
            {
                query = query.Where(i => i.DateOfEntry <= end_date.Value);
            }

            var incomingItem = await query
                .OrderByDescending(i => i.Id)
                .Select(i => new
                {
                    i.Id,
                    TransactionCode = i.TransactionCode,
                    DateOfEntry = i.DateOfEntry.ToString("yyyy-MM-dd"),
                    BatchNumber = i.BatchNumber,
                    StockIn = i.StockIn,
                    ExpiredDate = i.ExpiredDate.ToString("yyyy-MM-dd"),
                    TotalPurchase = i.TotalPurchase,
                    ItemName = i.Item.Name,
                    ItemUnit = i.Item.Unit.Name,
                    SupplierName = i.Supplier.Name
                })
                .ToListAsync();

            return Json(new { data = incomingItem });
        }



        // GET: Index
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // Print PDF
    public async Task<IActionResult> GetPdf([FromQuery] DateTime? start_date, [FromQuery] DateTime? end_date)
        {
            // Query data langsung dengan filtering di database
            var query = _context.IncomingItems
                .Include(i => i.Item)
                .Include(i => i.Supplier)
                .OrderByDescending(i => i.Id)
                .AsQueryable();

            if (start_date.HasValue)
            {
                query = query.Where(i => i.DateOfEntry >= start_date.Value);
            }
            if (end_date.HasValue)
            {
                query = query.Where(i => i.DateOfEntry <= end_date.Value);
            }

            var incomingItem = await query
                .Select(i => new
                {
                    i.Id,
                    i.TransactionCode,
                    DateOfEntry = i.DateOfEntry.ToString("yyyy-MM-dd"),
                    i.BatchNumber,
                    i.StockIn,
                    ExpiredDate = i.ExpiredDate.ToString("yyyy-MM-dd"),
                    i.TotalPurchase,
                    ItemName = i.Item.Name,
                    ItemUnit = i.Item.Unit.Name,
                    SupplierName = i.Supplier.Name
                })
                .ToListAsync();

            // Gunakan StringBuilder untuk membangun HTML
            var sb = new StringBuilder();
            sb.Append(@"
            <h1 style='text-align:center; font-family:Arial, sans-serif;'>Purchase Report</h1>
            <h4 style='text-align:center; font-family:Arial, sans-serif;'>Start Date: " + (start_date?.ToString("yyyy-MM-dd") ?? "-") + @" - " + (end_date?.ToString("yyyy-MM-dd") ?? "-") + @"</h4>
            <table style='width: 100%; border-collapse: collapse; text-align: left; font-family: Arial, sans-serif;'>
                <thead style='background-color: #f2f2f2;'>
                    <tr>
                        <th style='padding: 8px; border: 1px solid #ddd;'>No</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Transaction Code</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Batch Number</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Date</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Item Name</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Stock In</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Total Purchase</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Expired Date</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Supplier</th>
                    </tr>
                </thead>
                <tbody>");

            int index = 1;
            foreach (var item in incomingItem)
            {
                sb.AppendFormat(@"
                <tr style='border-bottom: 1px solid #ddd;'>
                    <td style='padding: 8px; text-align: center; border: 1px solid #ddd;'>{0}</td>
                    <td style='padding: 8px; text-align: center; border: 1px solid #ddd;'>{1}</td>
                    <td style='padding: 8px; text-align: center; border: 1px solid #ddd;'>{2}</td>
                    <td style='padding: 8px; text-align: center; border: 1px solid #ddd;'>{3}</td>
                    <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{4}</td>
                    <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{5}</td>
                    <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{6}</td>
                    <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{7}</td>
                    <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{8}</td>
                </tr>",
                    index++,
                    item.TransactionCode,
                    item.BatchNumber,
                    item.DateOfEntry,
                    item.ItemName,
                    item.StockIn,
                    item.TotalPurchase,
                    item.ExpiredDate,
                    item.SupplierName);
            }

            sb.Append("</tbody></table>");

            // Konversi HTML ke PDF dengan DinkToPdf
            var pdfConverter = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait
                },
                Objects = { new ObjectSettings { HtmlContent = sb.ToString(), WebSettings = { DefaultEncoding = "utf-8" } } }
            };

            var converter = HttpContext.RequestServices.GetRequiredService<IConverter>();
            var pdf = converter.Convert(pdfConverter);

            return File(pdf, "application/pdf", "Purchase_report.pdf");
        }

    }
}
