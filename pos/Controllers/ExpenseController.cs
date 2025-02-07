using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pos.Database;
using pos.Models;

namespace pos.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly AppDbContext _context;

        public ExpenseController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Expense
        public async Task<IActionResult> GetExpense()
        {
            var expense = await _context.FinancialHistories
                .Where(t => t.FinanceStatus == FinanceStatus.Out)
                .OrderByDescending(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Amount,
                    t.TransactionDate,
                    t.Description,
                    FinanceStatus = t.FinanceStatus.ToString()
                })
                .ToListAsync();
            return Json(new { data = expense });
        }

        // GET: Expense
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // GET: Expense/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Expense/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,TransactionDate,FinanceStatus,Description,FinanceId")] FinancialHistory financialHistory)
        {
            if (ModelState.IsValid)
            {
                // set finance status to out
                financialHistory.FinanceStatus = FinanceStatus.Out;

                // if finance status is out, then subtract the amount from the nominal
                var finance = await _context.Finances.FirstOrDefaultAsync();

                // if financial history more than the nominal, then return error
                if (finance.Nominal < financialHistory.Amount)
                {
                    return Json(new { success = false, message = "Nominal is not enough!" });
                }

                if (financialHistory.FinanceStatus == FinanceStatus.Out)
                {
                    finance.Nominal -= financialHistory.Amount;
                }

                // set finance id
                financialHistory.FinanceId = finance.Id;

                _context.Add(financialHistory);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Data has been saved!" });
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = errors });
        }


        // GET: Expense/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var financialHistory = await _context.FinancialHistories.FindAsync(id);
                    if (financialHistory == null)
                    {
                        return NotFound();
                    }

                    // if the finance status is in, then subtract the amount from the nominal
                    var finance = await _context.Finances.FirstOrDefaultAsync(f => f.Id == financialHistory.FinanceId);
                    if (financialHistory.FinanceStatus == FinanceStatus.Out)
                    {
                        finance.Nominal += financialHistory.Amount;
                    }
                    _context.FinancialHistories.Remove(financialHistory);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Data has been deleted!" });
                }
                catch (Exception e)
                {
                    return Json(new { success = false, message = e.Message });
                }

            }
        }

        private bool FinancialHistoryExists(int id)
        {
            return _context.FinancialHistories.Any(e => e.Id == id);
        }
    }
}
