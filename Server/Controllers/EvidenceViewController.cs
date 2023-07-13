using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Server;
using Server.Models;

namespace Server.Controllers
{
    public class EvidenceViewController : Controller
    {
        private readonly dbContext _context;

        public EvidenceViewController(dbContext context)
        {
            _context = context;
        }

        // GET: EvidenceView
        public async Task<IActionResult> Index(string steamID)
        {
              return _context.EvidenceModel != null ? 
                          View(await _context.EvidenceModel.Where(e=>e.steamId == steamID).ToListAsync()) :
                          Problem("Entity set 'dbContext.EvidenceModel'  is null.");
        }

        // GET: EvidenceView/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.EvidenceModel == null)
            {
                return NotFound();
            }

            var evidenceModel = await _context.EvidenceModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (evidenceModel == null)
            {
                return NotFound();
            }

            return View(evidenceModel);
        }

        // GET: EvidenceView/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EvidenceView/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,steamId,type,score,data,reasonForScore,createdAt,isProcessed")] EvidenceModel evidenceModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(evidenceModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(evidenceModel);
        }

        // GET: EvidenceView/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.EvidenceModel == null)
            {
                return NotFound();
            }

            var evidenceModel = await _context.EvidenceModel.FindAsync(id);
            if (evidenceModel == null)
            {
                return NotFound();
            }
            return View(evidenceModel);
        }

        // POST: EvidenceView/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,steamId,type,score,data,reasonForScore,createdAt,isProcessed")] EvidenceModel evidenceModel)
        {
            if (id != evidenceModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(evidenceModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EvidenceModelExists(evidenceModel.Id))
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
            return View(evidenceModel);
        }

        // GET: EvidenceView/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.EvidenceModel == null)
            {
                return NotFound();
            }

            var evidenceModel = await _context.EvidenceModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (evidenceModel == null)
            {
                return NotFound();
            }

            return View(evidenceModel);
        }

        // POST: EvidenceView/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.EvidenceModel == null)
            {
                return Problem("Entity set 'dbContext.EvidenceModel'  is null.");
            }
            var evidenceModel = await _context.EvidenceModel.FindAsync(id);
            if (evidenceModel != null)
            {
                _context.EvidenceModel.Remove(evidenceModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EvidenceModelExists(int id)
        {
          return (_context.EvidenceModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
