using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BiteBridge.Models;

namespace BiteBridge.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userEmail = User.Identity?.Name;

            var notifications = _context.EmailNotifications
                .Where(n => n.UserEmail == userEmail)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notifications);
        }

        public IActionResult MarkAsRead(int id)
        {
            var userEmail = User.Identity?.Name;

            var notification = _context.EmailNotifications
                .FirstOrDefault(n => n.Id == id && n.UserEmail == userEmail);

            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}