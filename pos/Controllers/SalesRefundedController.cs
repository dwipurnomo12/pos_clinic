using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pos.Database;
using pos.Models;

namespace pos.Controllers
{
    public class SalesRefundedController : Controller
    {
        private readonly AppDbContext _context;
        public SalesRefundedController(AppDbContext context)
        {
            _context = context;
        }
 
        // Get Sales Refunded
        public async Task<IActionResult> GetSalesRefunded()
        {
            var salesRefunded = await _context.SalesRefundeds
                .Include(s => s.Transaction)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
            return Json(new { data = salesRefunded  });
        }

        // Get Index
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions
                .Include(t => t.TransactionDetails) 
                .Where(t => t.Status == TransactionStatus.Completed)
                .Select(i => new
                {
                    i.Id,
                    DisplayText = $"{i.TransactionDetails.FirstOrDefault().ItemName} - {i.TransactionCode}"
                })
                .ToListAsync(); 

            ViewBag.Transactions = new SelectList(transactions, "Id", "DisplayText");
            return View();
        }

        // Get: SalesRefunded/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SalesRefunded/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TransactionId,StockReturned,Information")] SalesRefunded salesRefunded)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = errors });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get Data Transaction
                var transactionData = await _context.Transactions
                    .Include(t => t.TransactionDetails)
                    .FirstOrDefaultAsync(t => t.Id == salesRefunded.TransactionId);

                if (transactionData == null)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Transaction not found." });
                }

                if (transactionData.Status == TransactionStatus.Canceled)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Transaction has already been canceled." });
                }

                if (transactionData.Status != TransactionStatus.Completed)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Only paid transactions can be refunded." });
                }

                // Add data to table sales refunded
                _context.Add(salesRefunded);

                // Update status transaction
                transactionData.Status = TransactionStatus.Canceled;
                _context.Update(transactionData);

                // Update 
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Refund successfully processed." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // POST: SalesRefunded/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var salesRefunded = await _context.SalesRefundeds
                    .FirstOrDefaultAsync(sr => sr.Id == id);

                if (salesRefunded == null)
                {
                    return Json(new { success = false, message = "Refund not found." });
                }

                var transactionData = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == salesRefunded.TransactionId);

                if (transactionData == null)
                {
                    return Json(new { success = false, message = "Transaction not found." });
                }

                _context.SalesRefundeds.Remove(salesRefunded);

                if (transactionData.Status == TransactionStatus.Canceled)
                {
                    transactionData.Status = TransactionStatus.Completed;
                    _context.Transactions.Update(transactionData);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Refund successfully deleted and transaction status updated to Paid." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }


    }
}
