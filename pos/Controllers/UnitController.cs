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
    public class UnitController : Controller
    {
        private readonly AppDbContext _context;

        public UnitController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Unit
        public async Task<IActionResult> GetUnit()
        {
            var units = await _context.Units.ToListAsync();
            return Json(new { data = units });
        }

        //GET: Index
        public async Task<IActionResult> Index()
        {
            return View();
        }


        // GET: Unit/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Unit/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Unit unit)
        {
            if (ModelState.IsValid)
            {
                _context.Add(unit);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Unit added successfully!" });
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        // GET: Unit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unit = await _context.Units.FindAsync(id);
            if (unit == null)
            {
                return NotFound();
            }
            return Json(new { data = unit });
        }

        // POST: Unit/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Unit unit)
        {
            if (ModelState.IsValid)
            {
                _context.Update(unit);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Unit updated successfully!" });
        }

        // GET: Unit/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Units.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Units.Remove(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Unit deleted successfully!" });
        }

        private bool UnitExists(int id)
        {
            return _context.Units.Any(e => e.Id == id);
        }
    }
}
