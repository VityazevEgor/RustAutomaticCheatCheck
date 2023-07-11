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
	[Route("[controller]")]
	public class APIsusModelsController : Controller
    {
        private readonly dbContext _context;

        public APIsusModelsController(dbContext context)
        {
            _context = context;
        }

        // GET: APIsusModels
        public async Task<List<Models.SuspectsModel>> Index()
        {
            return _context.SuspectsModel != null ?
                        await _context.SuspectsModel.ToListAsync() :
                        null;
        }

		[HttpGet("Check/{steamId}")]
		public async Task<bool> Check(string steamId)
        {
            var model = await _context.SuspectsModel.FirstOrDefaultAsync(m => m.steamId == steamId);
            return model is not null;
        }

        private bool SuspectsModelExists(int id)
        {
          return (_context.SuspectsModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
