using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pos.Database;
using pos.Models;
using System.Text;

namespace pos.Controllers
{
    public class SalesReportController : Controller
    {
        protected readonly AppDbContext _context;
        public SalesReportController(AppDbContext context)
        {
            _context = context;
        }   

        // Get Sales Report
        public async Task<IActionResult> GetSalesReport([FromQuery] DateTime? start_date, [FromQuery] DateTime? end_date)
        {
            if (!start_date.HasValue || !end_date.HasValue)
            {
                return Json(new { message = "Select date range is required", data = new List<object>() });
            }
            var query = _context.Transactions
                .Include(i => i.Customer)
                .AsQueryable();
            if (start_date.HasValue)
            {
                query = query.Where(i => i.TransactionDate >= start_date.Value);
            }
            if (end_date.HasValue)
            {
                query = query.Where(i => i.TransactionDate <= end_date.Value);
            }
            var transactions = await query
                .OrderByDescending(i => i.Id)
                .Where(i => i.Status == TransactionStatus.Completed)
                .Select(i => new
                {
                    i.Id,
                    TransactionCode = i.TransactionCode,
                    TransactionDate = i.TransactionDate.ToString("yyyy-MM-dd"),
                    TotalAmount = i.TotalAmount,
                    PaymentMethod = i.PaymentMethod.ToString(),
                    Status = i.Status,
                    CustomerName = i.Customer.Name  
                })
                .ToListAsync();
            return Json(new { data = transactions });
        }

        // Get Index
        public IActionResult Index()
        {
            return View();
        }

        // Print PDF
        public async Task<IActionResult> getPdf([FromQuery] DateTime? start_date, [FromQuery] DateTime? end_date)
        {
            if (!start_date.HasValue || !end_date.HasValue)
            {
                return Json(new { message = "Select date range is required", data = new List<object>() });
            }
            var query = _context.Transactions
                .Include(i => i.Customer)
                .AsQueryable();
            if (start_date.HasValue)
            {
                query = query.Where(i => i.TransactionDate >= start_date.Value);
            }
            if (end_date.HasValue)
            {
                query = query.Where(i => i.TransactionDate <= end_date.Value);
            }
            var transactions = await query
                .OrderByDescending(i => i.Id)
                .Select(i => new
                {
                    i.Id,
                    TransactionCode = i.TransactionCode,
                    TransactionDate = i.TransactionDate.ToString("yyyy-MM-dd"),
                    TotalAmount = i.TotalAmount,
                    PaymentMethod = i.PaymentMethod.ToString(),
                    Status = i.Status,
                    CustomerName = i.Customer.Name
                })
                .ToListAsync();

            var sb = new StringBuilder();
            sb.Append(@"
            <h1 style='text-align:center; font-family:Arial, sans-serif;'>Purchase Report</h1>
            <h4 style='text-align:center; font-family:Arial, sans-serif;'>Start Date: " + (start_date?.ToString("yyyy-MM-dd") ?? "-") + @" - " + (end_date?.ToString("yyyy-MM-dd") ?? "-") + @"</h4>
            <table style='width: 100%; border-collapse: collapse; text-align: left; font-family: Arial, sans-serif;'>
                <thead style='background-color: #f2f2f2;'>
                    <tr>
                        <th style='padding: 8px; border: 1px solid #ddd;'>No</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Transaction Code</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Date</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Total Amount</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Payment Method</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Customer</th>
                    </tr>
                </thead>
                <tbody>");

            int index = 1;
            foreach (var transaction in transactions)
            {
                sb.AppendFormat(@"
                <tr style='border-bottom: 1px solid #ddd;'>
                    <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{0}</td>
                    <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{1}</td>
                    <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{2}</td>
                    <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{3}</td>
                    <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{4}</td>
                    <td style='padding: 8px; text-align: left; border: 1px solid #ddd;'>{5}</td>
                </tr>",
                    index++,
                    transaction.TransactionCode,
                    transaction.TransactionDate,
                    transaction.TotalAmount,
                    transaction.PaymentMethod,
                    transaction.CustomerName);
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

            return File(pdf, "application/pdf", "Sales_report.pdf");
        }
    }
}
