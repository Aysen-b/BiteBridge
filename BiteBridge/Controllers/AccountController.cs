using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BiteBridge.Models;
using BiteBridge.ViewModels;

namespace BiteBridge.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var allowedRoles = new[] { "User", "Caterer", "Admin" };

                if (!allowedRoles.Contains(model.RoleName))
                {
                    model.RoleName = "User";
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    RoleName = model.RoleName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    AddLog("Info", "Register", model.Email, $"New {model.RoleName} account registered.");

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    AddLog("Info", "Login", model.Email, "User automatically logged in after registration.");

                    return RedirectToAction("Dashboard", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                AddLog("Warning", "RegisterFailed", model.Email, "Registration failed.");
            }

            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                AddLog("Info", "LoginAttempt", model.Email, "User attempted to login.");

                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    isPersistent: false,
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    AddLog("Info", "LoginSuccess", model.Email, "User logged in successfully.");
                    return RedirectToAction("Dashboard", "Home");
                }

                AddLog("Warning", "LoginFailed", model.Email, "Invalid login attempt.");
                ModelState.AddModelError("", "Invalid login attempt");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            var email = User.Identity?.Name;

            await _signInManager.SignOutAsync();

            AddLog("Info", "Logout", email, "User logged out.");

            return RedirectToAction("Index", "Home");
        }

        private void AddLog(string level, string action, string? userEmail, string? details)
        {
            _context.SystemLogs.Add(new SystemLog
            {
                Level = level,
                Action = action,
                UserEmail = userEmail,
                Details = details
            });

            _context.SaveChanges();
        }
    }
}