using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BiteBridge.Models;
using Microsoft.EntityFrameworkCore;

namespace BiteBridge.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public HomeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public IActionResult Index()
{
    var menuItems = _context.MenuItems
        .Include(m => m.Caterer)
        .Include(m => m.Options)
        .OrderByDescending(m => m.Id)
        .ToList();

    return View(menuItems);
}

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (user.RoleName == "Admin")
        {
            return RedirectToAction("Index", "Admin");
        }

        if (user.RoleName == "Caterer")
        {
            return RedirectToAction("Index", "Caterer");
        }

        var userEmail = User.Identity?.Name;

        ViewBag.TotalOrders = _context.Orders.Count(o => o.UserEmail == userEmail);
        ViewBag.TotalSpent = _context.Orders
            .Where(o => o.UserEmail == userEmail)
            .Sum(o => (decimal?)o.TotalPrice) ?? 0;

        ViewBag.RecentOrders = _context.Orders
            .Where(o => o.UserEmail == userEmail)
            .OrderByDescending(o => o.Id)
            .Take(3)
            .ToList();

        return View();
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