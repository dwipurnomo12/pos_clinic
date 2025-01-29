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
    public class ItemController : Controller
    {
        private readonly AppDbContext _context;

        public ItemController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Item
        public async Task<IActionResult> GetItem()
        {
            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Unit)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
            return Json(new { data = item });
        }

        // GET: Index
        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.Units = new SelectList(_context.Units.ToList(), "Id", "Name");
            return View();
        }

        // GET: Item/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Unit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Item/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.Units = new SelectList(_context.Units.ToList(), "Id", "Name");
            return View();
        }

        // POST: Item/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,CategoryId,UnitId")] Item item)
        {
            if (ModelState.IsValid)
            {
                var random = new Random();
                int randomNumber = random.Next(10000, 99999);
                item.ItemCode = "ITM" + randomNumber;

                _context.Add(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Item successfully created!" });
            }
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.Units = new SelectList(_context.Units.ToList(), "Id", "Name");

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = errors });
        }

        // GET: Item/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Units = new SelectList(_context.Units, "Id", "Name");
            return Json(new { data = item });
        }

        // POST: Item/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,CategoryId,UnitId")] Item item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingItem = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                if (existingItem == null)
                {
                    return NotFound();
                }

                item.ItemCode = existingItem.ItemCode;

                _context.Update(item);
                await _context.SaveChangesAsync();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", item.CategoryId);
            ViewData["UnitId"] = new SelectList(_context.Units, "Id", "Name", item.UnitId);
            return Json(new { success = true, message = "Item updated successfully!" });
        }

        // GET: Item/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Something when wrong!" });
            }

            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return Json(new { success = false, message = "Something when wrong!" });
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();  

            return Json(new { success = true, message = "Item deleted successfully!" });
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
    }
}
