using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTE.Data;
using MyTE.Models;
using MyTE.Models.Enum;
using MyTE.Models.ViewModel;
using MyTE.Pagination;

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
        public async Task<IActionResult> Index(string searchString, int? pageNumber, WBSType? wbsType)
        {
            int pageSize = 7;
            ViewData["CurrentFilter"] = searchString;
            var wbs = from s in _context.WBS
                           select s;

            IQueryable<WBSType> wbsQuery = from m in _context.WBS
                                           orderby m.Type
                                           select m.Type;

            if (!string.IsNullOrEmpty(searchString))
            {
                wbs = wbs.Where(s => s.Code.Contains(searchString)
                                       || s.Desc.Contains(searchString));
            }

            if (wbsType.HasValue)
            {
                wbs = wbs.Where(x => x.Type == wbsType.Value);
            }

            var viewModel = new WBSViewModel
            {
                WBSList = await PaginatedList<WBS>.CreateAsync(wbs.AsNoTracking(), pageNumber ?? 1, pageSize),
                Type = new SelectList(await wbsQuery.Distinct().ToListAsync()),
                WBS = new WBS(),
                CurrentFilter = searchString
            };
            return View(viewModel);
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

        //GET: WBS/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var wBS = await _context.WBS
        //        .FirstOrDefaultAsync(m => m.WBSId == id);
        //    if (wBS == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(wBS);
        //}

        // POST: WBS/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
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
