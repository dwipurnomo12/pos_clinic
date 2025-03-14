﻿using System;
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
    public class IncomeController : Controller
    {
        private readonly AppDbContext _context;

        public IncomeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Income
        public async Task<IActionResult> GetIncome()
        {
            var income = await _context.FinancialHistories
                .Where(t => t.FinanceStatus == FinanceStatus.In)
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
            return Json(new { data = income });
        }

        // GET: Index
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // GET: Income/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Income/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,TransactionDate,FinanceStatus,Description")] FinancialHistory financialHistory)
        {
            if (ModelState.IsValid)
            {
                // set finance status to in
                financialHistory.FinanceStatus = FinanceStatus.In;

                // if the finance status is in, then add the amount to the nominal
                var finance = await _context.Finances.FirstOrDefaultAsync();
                if (financialHistory.FinanceStatus == FinanceStatus.In)
                {
                    finance.Nominal += financialHistory.Amount;
                }

                financialHistory.FinanceId = finance.Id;

                _context.Add(financialHistory);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Data has been saved!" });
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = errors });
        }

        
        // GET: Income/Delete/5
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
                    var finance = await _context.Finances.FirstOrDefaultAsync();
                    if (financialHistory.FinanceStatus == FinanceStatus.In)
                    {
                        finance.Nominal -= financialHistory.Amount;
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
