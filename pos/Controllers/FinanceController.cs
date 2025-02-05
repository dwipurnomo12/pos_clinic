using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pos.Database;
using pos.Models;

namespace pos.Controllers
{
    public class FinanceController : Controller
    {
        protected readonly AppDbContext _context;
        public FinanceController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var finance = await _context.Finances.SumAsync(t => t.Nominal);
            var income = await _context.FinancialHistories
                .Where(t => t.FinanceStatus == FinanceStatus.In)
                .SumAsync(t => t.Amount);

            var expense = await _context.FinancialHistories
                .Where(t => t.FinanceStatus == FinanceStatus.Out)
                .SumAsync(t => t.Amount);

            ViewBag.finance = finance; 
            ViewBag.income = income;
            ViewBag.expense = expense;
            return View();
        }

    }
}
