using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BiteBridge.Models;

namespace BiteBridge.Controllers
{
    [Authorize]
    public class CatererController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CatererController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.RoleName != "Caterer")
            {
                return RedirectToAction("Dashboard", "Home");
            }

            ViewBag.TotalOrders = _context.Orders.Count();
            ViewBag.CompletedOrders = _context.Orders.Count();
            ViewBag.RevenueSimulation = _context.Orders.Sum(o => (decimal?)o.TotalPrice) ?? 0;

            return View();
        }
    }
}