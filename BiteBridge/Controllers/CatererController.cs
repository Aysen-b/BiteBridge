using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BiteBridge.Models;

namespace BiteBridge.Controllers
{
    [Authorize]
    public class CatererController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public CatererController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.RoleName != "Caterer")
            {
                return RedirectToAction("Dashboard", "Home");
            }

            var caterer = await GetOrCreateCaterer(user.Email ?? "");

            ViewBag.TotalMenuItems = await _context.MenuItems.CountAsync(m => m.CatererId == caterer.Id);
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.CompletedOrders = await _context.Orders.CountAsync();
            ViewBag.RevenueSimulation = await _context.Orders.SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            return View();
        }

        public async Task<IActionResult> MenuItems()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.RoleName != "Caterer")
            {
                return RedirectToAction("Dashboard", "Home");
            }

            var caterer = await GetOrCreateCaterer(user.Email ?? "");

            var items = await _context.MenuItems
                .Include(m => m.Options)
                .Where(m => m.CatererId == caterer.Id)
                .OrderByDescending(m => m.Id)
                .ToListAsync();

            return View(items);
        }
        public async Task<IActionResult> Profile()
{
    var user = await _userManager.GetUserAsync(User);

    if (user == null || user.RoleName != "Caterer")
    {
        return RedirectToAction("Dashboard", "Home");
    }

    var caterer = await GetOrCreateCaterer(user.Email ?? "");

    return View(caterer);
}

[HttpPost]
public async Task<IActionResult> Profile(
    string businessName,
    string description,
    string address,
    double latitude,
    double longitude)
{
    var user = await _userManager.GetUserAsync(User);

    if (user == null || user.RoleName != "Caterer")
    {
        return RedirectToAction("Dashboard", "Home");
    }

    var caterer = await GetOrCreateCaterer(user.Email ?? "");

    caterer.BusinessName = businessName;
    caterer.Description = description;
    caterer.Address = address;
    caterer.Latitude = latitude;
    caterer.Longitude = longitude;

    _context.SystemLogs.Add(new SystemLog
    {
        Level = "Info",
        Action = "CatererLocationUpdated",
        UserEmail = user.Email,
        Details = $"Caterer location updated: {address}"
    });

    await _context.SaveChangesAsync();

    return RedirectToAction("Index");
}

        public async Task<IActionResult> CreateMenuItem()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.RoleName != "Caterer")
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenuItem(
            string name,
            string description,
            decimal price,
            IFormFile? imageFile,
            string? optionNames,
            string? optionTypes,
            string? optionPrices)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.RoleName != "Caterer")
            {
                return RedirectToAction("Dashboard", "Home");
            }

            var caterer = await GetOrCreateCaterer(user.Email ?? "");

            string? imageUrl = null;

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                imageUrl = "/uploads/" + fileName;
            }

            var menuItem = new MenuItem
            {
                Name = name,
                Description = description,
                Price = price,
                ImageUrl = imageUrl,
                CatererId = caterer.Id
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            AddOptions(menuItem.Id, optionNames, optionTypes, optionPrices);

            _context.SystemLogs.Add(new SystemLog
            {
                Level = "Info",
                Action = "MenuItemCreated",
                UserEmail = user.Email,
                Details = $"Menu item '{name}' was created by caterer."
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("MenuItems");
        }

        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.RoleName != "Caterer")
            {
                return RedirectToAction("Dashboard", "Home");
            }

            var caterer = await GetOrCreateCaterer(user.Email ?? "");

            var item = await _context.MenuItems
                .Include(m => m.Options)
                .FirstOrDefaultAsync(m => m.Id == id && m.CatererId == caterer.Id);

            if (item != null)
            {
                _context.MenuItemOptions.RemoveRange(item.Options);
                _context.MenuItems.Remove(item);

                _context.SystemLogs.Add(new SystemLog
                {
                    Level = "Warning",
                    Action = "MenuItemDeleted",
                    UserEmail = user.Email,
                    Details = $"Menu item '{item.Name}' was deleted."
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MenuItems");
        }

        private async Task<Caterer> GetOrCreateCaterer(string email)
        {
            var caterer = await _context.Caterers.FirstOrDefaultAsync(c => c.OwnerEmail == email);

            if (caterer == null)
            {
                caterer = new Caterer
                {
                    BusinessName = "BiteBridge Caterer",
                    Description = "Demo catering restaurant account.",
                    Address = "Ankara",
                    OwnerEmail = email,
                    Latitude = 39.9208,
                    Longitude = 32.8541
                };

                _context.Caterers.Add(caterer);
                await _context.SaveChangesAsync();
            }

            return caterer;
        }

        private void AddOptions(int menuItemId, string? optionNames, string? optionTypes, string? optionPrices)
        {
            if (string.IsNullOrWhiteSpace(optionNames))
            {
                return;
            }

            var names = optionNames.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var types = optionTypes?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var prices = optionPrices?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            for (int i = 0; i < names.Length; i++)
            {
                decimal extraPrice = 0;

                if (i < prices.Length)
                {
                    decimal.TryParse(prices[i].Trim(), out extraPrice);
                }

                var option = new MenuItemOption
                {
                    MenuItemId = menuItemId,
                    OptionName = names[i].Trim(),
                    OptionType = i < types.Length ? types[i].Trim() : "Optional Addition",
                    ExtraPrice = extraPrice
                };

                _context.MenuItemOptions.Add(option);
            }
        }
    }
}