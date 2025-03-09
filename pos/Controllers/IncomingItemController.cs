using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pos.Database;
using pos.Models;

namespace pos.Controllers
{
    public class IncomingItemController : Controller
    {
        private readonly AppDbContext _context;

        public IncomingItemController(AppDbContext context)
        {
            _context = context;
        }

        //GET: IncomingItem
        public async Task<IActionResult> GetIncomingItem()
        {
            var incomingItem = await _context.IncomingItems
                .Include(i => i.Item)
                .Include(i => i.Supplier)
                .OrderByDescending(i => i.Id)
                .Select(i => new
                {
                    i.Id,
                    transactionCode = i.TransactionCode,
                    dateOfEntry = i.DateOfEntry.ToString("yyyy-MM-dd"),
                    batchNumber = i.BatchNumber,
                    stockIn = i.StockIn,
                    expiredDate = i.ExpiredDate.ToString("yyyy-MM-dd"),
                    totalPurchase = i.TotalPurchase,
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
            ViewBag.Items = new SelectList(_context.Items.ToList(), "Id", "Name");
            ViewBag.Suppliers = new SelectList(_context.Suppliers.ToList(), "Id", "Name");
            return View();
        }


        // GET: IncomingItem/Create
        public IActionResult Create()
        {
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name");
            return View();
        }

        // POST: IncomingItem/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DateOfEntry,TransactionCode,BatchNumber,StockIn,ExpiredDate,TotalPurchase,ItemId,SupplierId")] IncomingItem incomingItem)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = errors });
            }

            // Mulai transaksi
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Generate Transaction Code
                var random = new Random();
                int randomNumber = random.Next(10000, 99999);
                incomingItem.TransactionCode = "INC-" + randomNumber;

                // Set DateOfEntry ke waktu saat ini
                var dateNow = DateTime.Now;
                incomingItem.DateOfEntry = dateNow;

                // Ambil batch number terakhir untuk ItemId terkait
                var lastBatch = await _context.IncomingItems
                    .Where(i => i.ItemId == incomingItem.ItemId)
                    .OrderByDescending(i => i.BatchNumber)
                    .Select(i => i.BatchNumber)
                    .FirstOrDefaultAsync();

                // Tentukan batch number berikutnya
                int nextBatchNumber = 1;
                if (!string.IsNullOrEmpty(lastBatch) && lastBatch.StartsWith("Batch"))
                {
                    if (int.TryParse(lastBatch.Replace("Batch", ""), out int lastBatchNum))
                    {
                        nextBatchNumber = lastBatchNum + 1;
                    }
                }
                incomingItem.BatchNumber = "Batch" + nextBatchNumber.ToString("D4");

                // Update stok item
                var item = await _context.Items.FindAsync(incomingItem.ItemId);
                if (item == null)
                {
                    return Json(new { success = false, message = "Item not found!" });
                }
                item.stock += incomingItem.StockIn;
                _context.Update(item);

                // Cek keuangan
                var finance = await _context.Finances.FirstOrDefaultAsync();
                if (finance == null || finance.Nominal < (incomingItem.TotalPurchase ?? 0))
                {
                    return Json(new { success = false, message = "Finance nominal is not enough!" });
                }

                // Kurangi saldo keuangan
                finance.Nominal -= incomingItem.TotalPurchase ?? 0;
                _context.Update(finance);

                // Tambahkan ke riwayat keuangan
                var expense = new FinancialHistory
                {
                    FinanceId = finance.Id,
                    TransactionDate = dateNow,
                    Amount = incomingItem.TotalPurchase ?? 0,
                    FinanceStatus = FinanceStatus.Out,
                    Description = "Purchase Item " + item.Name
                };
                _context.Add(expense);

                // Simpan semua perubahan dalam satu transaksi
                _context.Add(incomingItem);
                await _context.SaveChangesAsync();

                // Commit transaksi jika semua sukses
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Data has been saved successfully!" });
            }
            catch (Exception ex)
            {
                // Rollback jika ada error
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


        // GET: IncomingItem/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incomingItem = await _context.IncomingItems.FindAsync(id);
            if (incomingItem == null)
            {
                return NotFound();
            }
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name", incomingItem.ItemId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", incomingItem.SupplierId);
            return Json(new {data = incomingItem});
        }

        // POST: IncomingItem/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StockIn,ExpiredDate,ItemId,SupplierId,DateOfEntry,TotalPurchase,TransactionCode,BatchNumber")] IncomingItem incomingItem)
        {
            if (id != incomingItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existingItem = await _context.IncomingItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                    if (existingItem == null)
                    {
                        return NotFound();
                    }

                    incomingItem.DateOfEntry = existingItem.DateOfEntry;
                    incomingItem.TransactionCode = existingItem.TransactionCode;
                    incomingItem.BatchNumber = existingItem.BatchNumber;

                    int stockInDiff = incomingItem.StockIn - existingItem.StockIn;

                    var item = await _context.Items.FindAsync(incomingItem.ItemId);
                    if (item == null)
                    {
                        return NotFound();
                    }
                    item.stock += stockInDiff;
                    _context.Update(item);

                    decimal totalPurchaseDiff = (incomingItem.TotalPurchase ?? 0) - (existingItem.TotalPurchase ?? 0);

                    var finance = await _context.Finances.FirstOrDefaultAsync();
                    if (finance == null)
                    {
                        return Json(new { success = false, message = "Finance data not found!" });
                    }

                    if (totalPurchaseDiff > 0 && finance.Nominal < totalPurchaseDiff)
                    {
                        return Json(new { success = false, message = "Finance nominal is not enough to cover the update!" });
                    }

                    finance.Nominal -= totalPurchaseDiff;
                    _context.Update(finance);

                    if (totalPurchaseDiff != 0)
                    {
                        var expense = await _context.FinancialHistories
                            .FirstOrDefaultAsync(f => f.FinanceId == finance.Id && f.TransactionDate == existingItem.DateOfEntry);

                        if (expense != null)
                        {
                            expense.Amount += totalPurchaseDiff;
                            _context.Update(expense);
                        }
                        else
                        {
                            var newExpense = new FinancialHistory
                            {
                                FinanceId = finance.Id,
                                TransactionDate = existingItem.DateOfEntry,
                                Amount = incomingItem.TotalPurchase ?? 0,
                                FinanceStatus = FinanceStatus.Out,
                                Description = "Updated Purchase of Item " + (item != null ? item.Name : "Unknown Item")
                            };
                            _context.Add(newExpense);
                        }
                    }

                    _context.Update(incomingItem);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Incoming Item updated successfully!" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(); 
                    return Json(new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            // Jika ModelState tidak valid, kembalikan error
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = errors });
        }


        // GET: IncomingItem/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Something went wrong!" });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var incomingItem = await _context.IncomingItems.FindAsync(id);
                    if (incomingItem == null)
                    {
                        return Json(new { success = false, message = "Something went wrong!" });
                    }

                    var item = await _context.Items.FindAsync(incomingItem.ItemId);
                    if (item != null)
                    {
                        item.stock -= incomingItem.StockIn; 
                        _context.Items.Update(item);
                    }

                    _context.IncomingItems.Remove(incomingItem);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Item deleted successfully!" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Error: " + ex.Message });
                }
            }
        }



        private bool IncomingItemExists(int id)
        {
            return _context.IncomingItems.Any(e => e.Id == id);
        }
    }
}
