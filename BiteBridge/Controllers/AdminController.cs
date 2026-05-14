using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BiteBridge.Models;

namespace BiteBridge.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Logs(string? search, string? level, int page = 1)
        {
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
            ViewBag.TotalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);

            return View(logs);
        }
    }
}