using Microsoft.AspNetCore.Mvc;
using BiteBridge.Models;

namespace BiteBridge.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Checkout([FromBody] Order order)
        {
            order.UserEmail = User.Identity.Name;

            _context.Orders.Add(order);
            _context.SaveChanges();

            return Ok();
        }
        public IActionResult List()
        {
            var userEmail = User.Identity.Name;

            var orders = _context.Orders
                .Where(o => o.UserEmail == userEmail)
                .OrderByDescending(o => o.Id)
                .ToList();

            return View(orders);
        }
        public IActionResult Delete(int id)
        {
        var userEmail = User.Identity.Name;

        var order = _context.Orders
            .FirstOrDefault(o => o.Id == id && o.UserEmail == userEmail);

        if (order != null)
        {
            _context.Orders.Remove(order);
            _context.SaveChanges();
        }

            return RedirectToAction("List");
        }
    }
}