using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BiteBridge.Models;

namespace BiteBridge.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.RoleName != "Admin")
            {
                return RedirectToAction("Dashboard", "Home");
            }

            ViewBag.TotalOrders = _context.Orders.Count();
            ViewBag.TotalLogs = _context.SystemLogs.Count();
            ViewBag.TotalUsers = _userManager.Users.Count();

            return View();
        }

        public async Task<IActionResult> Logs(string? search, string? level, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.RoleName != "Admin")
            {
                return RedirectToAction("Dashboard", "Home");
            }

            int pageSize = 10;

            var query = _context.SystemLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(log =>
                    log.Action.Contains(search) ||
                    (log.UserEmail != null && log.UserEmail.Contains(search)) ||
                    (log.Details != null && log.Details.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(level))
            {
                query = query.Where(log => log.Level == level);
            }

            int totalLogs = query.Count();

            var logs = query
                .OrderByDescending(log => log.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.Level = level;
            ViewBag.Page = page;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(totalLogs / (double)pageSize));

            return View(logs);
        }
    }
}