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
    public class APIEvidenceController : Controller
    {
        private readonly dbContext _context;

        public APIEvidenceController(dbContext context)
        {
            _context = context;
        }

        // GET: APIEvidence
        public async Task<IActionResult> Index()
        {
              return _context.EvidenceModel != null ? 
                          View(await _context.EvidenceModel.ToListAsync()) :
                          Problem("Entity set 'dbContext.EvidenceModel'  is null.");
        }

        [HttpPost]
        public async Task<bool> getEvidence(string steamId,string type, string data)
        {
            if (await _context.SuspectsModel.FirstOrDefaultAsync(s => s.steamId == steamId) is not null && await _context.EvidenceModel.FirstOrDefaultAsync(e => e.type == type) is null)
            {
                var newEv = new EvidenceModel();
                newEv.steamId = steamId;
                newEv.type = type;
                newEv.data = data;
                await _context.EvidenceModel.AddAsync(newEv);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
