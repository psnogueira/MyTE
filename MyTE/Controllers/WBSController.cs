using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTE.Data;
using MyTE.Models;

namespace MyTE.Controllers
{
    public class WBSController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WBSController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WBS
        public async Task<IActionResult> Index()
        {
            return View(await _context.WBS.ToListAsync());
        }

        // GET: WBS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wBS = await _context.WBS
                .FirstOrDefaultAsync(m => m.WBSId == id);
            if (wBS == null)
            {
                return NotFound();
            }

            return View(wBS);
        }

        // GET: WBS/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WBS/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WBSId,Code,Desc,Type")] WBS wBS)
        {
            if (ModelState.IsValid)
            {
                _context.Add(wBS);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(wBS);
        }

        // GET: WBS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wBS = await _context.WBS.FindAsync(id);
            if (wBS == null)
            {
                return NotFound();
            }
            return View(wBS);
        }

        // POST: WBS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WBSId,Code,Desc,Type")] WBS wBS)
        {
            if (id != wBS.WBSId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wBS);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WBSExists(wBS.WBSId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(wBS);
        }

        // GET: WBS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wBS = await _context.WBS
                .FirstOrDefaultAsync(m => m.WBSId == id);
            if (wBS == null)
            {
                return NotFound();
            }

            return View(wBS);
        }

        // POST: WBS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wBS = await _context.WBS.FindAsync(id);
            if (wBS != null)
            {
                _context.WBS.Remove(wBS);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WBSExists(int id)
        {
            return _context.WBS.Any(e => e.WBSId == id);
        }
    }
}
