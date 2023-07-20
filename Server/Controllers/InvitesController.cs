using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Filters;
using Server.Models;

namespace Server.Controllers
{
    [TypeFilter(typeof(AuthFilter))]
    public class InvitesController : Controller
    {
        private readonly dbContext _context;

        public InvitesController(dbContext context)
        {
            _context = context;
        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
              return _context.InviteModel != null ? 
                          View(await _context.InviteModel.ToListAsync()) :
                          Problem("Entity set 'dbContext.InviteModel'  is null.");
        }


        // GET: Invites/Create
        public async Task<IActionResult> Create()
        {
            await _context.InviteModel.AddAsync(new InviteModel { Code = Path.GetRandomFileName(), isUsed = false});
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Invites/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.InviteModel == null)
            {
                return NotFound();
            }

            var inviteModel = await _context.InviteModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inviteModel == null)
            {
                return NotFound();
            }

            return View(inviteModel);
        }

        // POST: Invites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.InviteModel == null)
            {
                return Problem("Entity set 'dbContext.InviteModel'  is null.");
            }
            var inviteModel = await _context.InviteModel.FindAsync(id);
            if (inviteModel != null)
            {
                _context.InviteModel.Remove(inviteModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InviteModelExists(int id)
        {
          return (_context.InviteModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
