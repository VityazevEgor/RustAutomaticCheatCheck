using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using System.Diagnostics;

namespace Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly dbContext _context;
        public HomeController(ILogger<HomeController> logger, dbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.SuspectsModel.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> createNewSus(string steamId)
        {
            var model = new SuspectsModel { steamId = steamId };
            await _context.SuspectsModel.AddAsync(model);
            await _context.SaveChangesAsync();
			return RedirectToAction("Index");
		}

        [HttpGet]
        public async Task<IActionResult> deleteSus(int id)
        {
            var model = await _context.SuspectsModel.FirstOrDefaultAsync(m=>m.Id == id);
            if (model is not null)
            {
                _context.SuspectsModel.Remove(model);
                await _context.SaveChangesAsync();
            }
			return RedirectToAction("Index");
		}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}