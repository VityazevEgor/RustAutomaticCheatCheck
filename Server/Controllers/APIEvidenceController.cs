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
        public async Task<string> getEvidence(string steamId, string type, string data)
        {
            if (string.IsNullOrEmpty(steamId))
            {
                return "No steam Id";
            }
            if (string.IsNullOrEmpty(type))
            {
                return "No type";
            }
            if (string.IsNullOrEmpty(data) )
            {
                return "No data";
            }
            var testSuspect = await _context.SuspectsModel.FirstOrDefaultAsync(s => s.steamId == steamId);
			if (testSuspect is null)
            {
                return "There is no suspect with such steam ID " + steamId;
            }
            var testEvidence = await _context.EvidenceModel.FirstOrDefaultAsync(e => e.type == type && e.steamId == steamId);
            if (testEvidence is not null)
            {
                return "There is already evidence with type - " + type;
            }
			var newEv = new EvidenceModel();
			newEv.steamId = steamId;
			newEv.type = type;
			newEv.data = data;
			await _context.EvidenceModel.AddAsync(newEv);
			await _context.SaveChangesAsync();
			return "true";
		}
    }
}
