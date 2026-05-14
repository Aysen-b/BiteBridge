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

                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var passwordIsValid = await _userManager.CheckPasswordAsync(user, model.Password);

                    if (passwordIsValid)
                    {
                        var code = new Random().Next(100000, 999999).ToString();

                        TempData["TwoFactorEmail"] = model.Email;
                        TempData["TwoFactorCode"] = code;

                        AddLog("Info", "TwoFactorCodeGenerated", model.Email, "2FA verification code generated.");

                        return RedirectToAction("VerifyTwoFactor");
                    }
                }

                AddLog("Warning", "LoginFailed", model.Email, "Invalid login attempt.");
                ModelState.AddModelError("", "Invalid login attempt");
            }

            return View(model);
        }

        public IActionResult VerifyTwoFactor()
        {
            var email = TempData["TwoFactorEmail"]?.ToString();
            var code = TempData["TwoFactorCode"]?.ToString();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                return RedirectToAction("Login");
            }

            ViewBag.Email = email;
            ViewBag.DemoCode = code;

            TempData.Keep("TwoFactorEmail");
            TempData.Keep("TwoFactorCode");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyTwoFactor(string verificationCode)
        {
            var email = TempData["TwoFactorEmail"]?.ToString();
            var correctCode = TempData["TwoFactorCode"]?.ToString();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(correctCode))
            {
                return RedirectToAction("Login");
            }

            if (verificationCode == correctCode)
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                await _signInManager.SignInAsync(user, isPersistent: false);

                AddLog("Info", "TwoFactorSuccess", email, "User completed 2FA verification.");
                AddLog("Info", "LoginSuccess", email, "User logged in successfully after 2FA.");

                TempData.Remove("TwoFactorEmail");
                TempData.Remove("TwoFactorCode");

                return RedirectToAction("Dashboard", "Home");
            }

            ViewBag.Email = email;
            ViewBag.DemoCode = correctCode;
            ViewBag.Error = "Invalid verification code.";

            AddLog("Warning", "TwoFactorFailed", email, "User entered invalid 2FA verification code.");

            TempData.Keep("TwoFactorEmail");
            TempData.Keep("TwoFactorCode");

            return View();
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