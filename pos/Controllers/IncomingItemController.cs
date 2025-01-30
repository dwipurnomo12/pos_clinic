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
        public async Task<IActionResult> Create([Bind("Id,DateOfEntry,TransactionCode,BatchNumber,StockIn,ExpiredDate,ItemId,SupplierId")] IncomingItem incomingItem)
        {
            if (ModelState.IsValid)
            {
                // Generate Transaction Code
                var random = new Random();
                int randomNumber = random.Next(10000, 99999);
                incomingItem.TransactionCode = "INC-" + randomNumber;

                // Date now
                var dateNow = DateTime.Now;
                incomingItem.DateOfEntry = dateNow;

                // Get last batch number
                var lastBatch = await _context.IncomingItems
                    .Where(i => i.ItemId == incomingItem.ItemId) // Filter by itemId
                    .OrderByDescending(i => i.BatchNumber)
                    .Select(i => i.BatchNumber)
                    .FirstOrDefaultAsync();

                int nextBatchNumber = 1;

                // if last batch is not null and start with "Batch"
                if (!string.IsNullOrEmpty(lastBatch) && lastBatch.StartsWith("Batch"))
                {
                    // Get last batch number
                    string lastBatchNumStr = lastBatch.Replace("Batch", "");

                    if (int.TryParse(lastBatchNumStr, out int lastBatchNum))
                    {
                        nextBatchNumber = lastBatchNum + 1; // Add 1 to the last batch number
                    }
                }

                // Format batch number 
                incomingItem.BatchNumber = "Batch" + nextBatchNumber.ToString("D4");

                //Update stock in Item
                var item = await _context.Items.FindAsync(incomingItem.ItemId);
                if(item != null)
                {
                    item.stock += incomingItem.StockIn;
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }

                // Save to database
                _context.Add(incomingItem);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Data has been saved" });
            }

            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name", incomingItem.ItemId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", incomingItem.SupplierId);

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = errors });
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,StockIn,ExpiredDate,ItemId,SupplierId,DateOfEntry,TransactionCode,BatchNumber")] IncomingItem incomingItem)
        {
            if (id != incomingItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingItem = await _context.IncomingItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                if (existingItem == null)
                {
                    return NotFound();
                }

                // Update DateOfEntry, TransactionCode, and BatchNumber from the existing item
                incomingItem.DateOfEntry = existingItem.DateOfEntry;
                incomingItem.TransactionCode = existingItem.TransactionCode;
                incomingItem.BatchNumber = existingItem.BatchNumber;

                int stockInDiff = incomingItem.StockIn - existingItem.StockIn;

                // Update the item stock
                var item = await _context.Items.FindAsync(incomingItem.ItemId);
                if (item != null)
                {
                    item.stock += stockInDiff;
                    _context.Update(item);
                    await _context.SaveChangesAsync(); // Save item update

                    // Update incoming item
                    _context.Update(incomingItem);
                    await _context.SaveChangesAsync(); // Save incoming item update
                }
                else
                {
                    return NotFound(); // Item not found
                }

                return Json(new { success = true, message = "Incoming Item updated successfully!" });
            }

            // If model state is invalid, return errors
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name", incomingItem.ItemId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", incomingItem.SupplierId);
            return Json(new { success = false, message = "Invalid data submitted" });
        }

        // GET: IncomingItem/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Something when wrong!" });
            }

            var incomingItem = await _context.IncomingItems.FindAsync(id);
            if (incomingItem == null)
            {
                return Json(new { success = false, message = "Something when wrong!" });
            }

            var item = await _context.Items.FindAsync(incomingItem.ItemId);
            if (item != null)
            {
                item.stock -= incomingItem.StockIn;
                _context.Update(item);
                await _context.SaveChangesAsync();
            }

            _context.IncomingItems.Remove(incomingItem);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Item deleted successfully!" });
        }


        private bool IncomingItemExists(int id)
        {
            return _context.IncomingItems.Any(e => e.Id == id);
        }
    }
}
