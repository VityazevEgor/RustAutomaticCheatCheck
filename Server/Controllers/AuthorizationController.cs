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
    public class AuthorizationController : Controller
    {
        private readonly dbContext _context;

        public AuthorizationController(dbContext context)
        {
            _context = context;
        }

        // GET: Authorization
        public async Task<IActionResult> Index()
        {
              return View();
        }

        public async Task<IActionResult> Login([Bind("Login,Password")] ModeratorModel moderatorModel)
        {
            var check = await _context.ModeratorModel.FirstOrDefaultAsync(m=>m.Login == moderatorModel.Login && m.Password == moderatorModel.Password);
            if (check is not null)
            {
                HttpContext.Session.SetString("isAuth", "True");
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Authorization/Create
        public async Task<IActionResult> Create()
        {
            var invCode = await _context.InviteModel.FirstOrDefaultAsync(i => i.Code == "firstInvite");
            if (invCode is null)
            {
                await _context.InviteModel.AddAsync(new InviteModel { Code = "firstInvite", isUsed = false });
                await _context.SaveChangesAsync();
            }
            return View();
        }

        // POST: Authorization/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Login,Password")] ModeratorModel moderatorModel, string InviteCode)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(InviteCode) && (await _context.ModeratorModel.FirstOrDefaultAsync(m=>m.Login == moderatorModel.Login)) is null)
            {
                var invCode = await _context.InviteModel.FirstOrDefaultAsync(m => m.Code == InviteCode && !m.isUsed);
                if (invCode is not null)
                {
                    _context.Add(moderatorModel);
                    invCode.isUsed = true;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(moderatorModel);
        }

        

        private bool ModeratorModelExists(int id)
        {
          return (_context.ModeratorModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
