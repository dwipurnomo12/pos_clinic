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
    public class ItemReturnedController : Controller
    {
        private readonly AppDbContext _context;

        public ItemReturnedController(AppDbContext context)
        {
            _context = context;
        }

        //GET : ItemReturned
        public async Task<IActionResult> GetItemReturned()
        {
            var itemReturned = await _context.ItemReturneds
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
            return Json(new { data = itemReturned });
        }

        // GET: Index
        public async Task<IActionResult> Index()
        {
            var incomingItemsList = _context.IncomingItems
                .Include(i => i.Item)
                .ToList()
                .Select(i => new
                {
                    Id = i.Id,
                    DisplayText = $"{i.Item.Name} - {i.BatchNumber}"
                })
                .ToList();

            ViewBag.IncomingItems = new SelectList(incomingItemsList, "Id", "DisplayText");
            return View();
        }


        // GET: ItemReturned/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ItemReturned/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IncomingItemId,StockReturned,Information")] ItemReturned itemReturned)
        {
            if (ModelState.IsValid)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Get Data incomingItem
                    var incomingItem = await _context.IncomingItems.FindAsync(itemReturned.IncomingItemId);
                    if (incomingItem == null)
                    {
                        return Json(new { success = false, message = "Incoming item not found" });
                    }
                    // Check stok
                    if (itemReturned.StockReturned > incomingItem.StockIn)
                    {
                        return Json(new { success = false, message = "Stock returned cannot be greater than available stock" });
                    }
                    // Calculate stock returned difference
                    int stockReturnedDiff = itemReturned.StockReturned;
                    // Stock cannot be negative
                    if (incomingItem.StockIn - stockReturnedDiff < 0)
                    {
                        return Json(new { success = false, message = "Stock cannot be negative" });
                    }
                    // Update stock incoming item
                    incomingItem.StockIn -= stockReturnedDiff;
                    _context.Update(incomingItem);
                    // Get Data Incoming Item
                    var item = await _context.Items.FindAsync(incomingItem.ItemId);
                    if (item != null)
                    {
                        item.stock -= stockReturnedDiff;
                        _context.Update(item);
                    }
                    // Save item returned
                    _context.Add(itemReturned);
                    await _context.SaveChangesAsync();
                    // Commit transaction
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "Data has been saved" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "An error occurred: " + ex.Message });
                }
                   
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = errors });

        }

        // GET: ItemReturned/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemReturned = await _context.ItemReturneds
                .Where(i => i.Id == id)
                .Select(i => new
                {
                    i.Id,
                    IncomingItemId = i.IncomingItemId,
                    ItemName = i.IncomingItem.Item.Name,
                    batchNumber = i.IncomingItem.BatchNumber,
                    stockReturned = i.StockReturned,
                    information = i.Information
                })
                .FirstOrDefaultAsync(); 

            if (itemReturned == null)
            {
                return NotFound();
            }

            var incomingItems = _context.IncomingItems
               .Include(i => i.Item)
               .Select(i => new { i.Id, DisplayText = $"{i.Item.Name} - {i.BatchNumber}" })
               .ToList();

            return Json(new { data = itemReturned });
        }


        // POST: ItemReturned/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IncomingItemId,StockReturned,Information")] ItemReturned itemReturned)
        {
            if (id != itemReturned.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existingItem = await _context.ItemReturneds.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                    if (existingItem == null)
                    {
                        return NotFound();
                    }

                    // Get Data incomingItem
                    var incomingItem = await _context.IncomingItems.FindAsync(itemReturned.IncomingItemId);
                    if (incomingItem == null)
                    {
                        return Json(new { success = false, message = "Incoming item not found" });
                    }

                    // Check stok
                    if (itemReturned.StockReturned > incomingItem.StockIn)
                    {
                        return Json(new { success = false, message = "Stock returned cannot be greater than available stock" });
                    }

                    // Calculate stock returned difference
                    int stockReturnedDiff = itemReturned.StockReturned - existingItem.StockReturned;

                    // Stock cannot be negative
                    if (incomingItem.StockIn - stockReturnedDiff < 0)
                    {
                        return Json(new { success = false, message = "Stock cannot be negative" });
                    }

                    // Update stock incoming item
                    incomingItem.StockIn -= stockReturnedDiff;
                    _context.Update(incomingItem);

                    // Get Data Incoming Item
                    var item = await _context.Items.FindAsync(incomingItem.ItemId);
                    if (item != null)
                    {
                        item.stock -= stockReturnedDiff;
                        _context.Update(item);
                    }

                    // Update item returned
                    _context.Update(itemReturned);

                    // Save all changes in one transaction
                    await _context.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Data has been updated" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            ViewData["IncomingItemId"] = new SelectList(_context.IncomingItems, "Id", "Id", itemReturned.IncomingItemId);
            return Json(new { success = false, message = "Invalid data" });
        }



        // GET: ItemReturned/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var itemReturned = await _context.ItemReturneds
                        .Include(i => i.IncomingItem)
                        .FirstOrDefaultAsync(m => m.Id == id);
                    if (itemReturned == null)
                    {
                        return NotFound();
                    }
                    //update stock incoming item
                    var incomingItem = await _context.IncomingItems.FindAsync(itemReturned.IncomingItemId);
                    if (incomingItem != null)
                    {
                        incomingItem.StockIn += itemReturned.StockReturned;
                        _context.Update(incomingItem);
                        await _context.SaveChangesAsync();
                    }
                    //update total stock in item
                    var item = await _context.Items.FindAsync(incomingItem.ItemId);
                    if (item != null)
                    {
                        item.stock += itemReturned.StockReturned;
                        _context.Update(item);
                        await _context.SaveChangesAsync();
                    }
                    _context.ItemReturneds.Remove(itemReturned);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return Json(new { success = true, message = "Data has been deleted!" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }


        private bool ItemReturnedExists(int id)
        {
            return _context.ItemReturneds.Any(e => e.Id == id);
        }
    }
}
