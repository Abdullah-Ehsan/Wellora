using BCrypt.Net;
using Org.BouncyCastle.Crypto.Generators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wellora.Models;
using Wellora.Data;
using Wellora.Areas.Admin.Models;
using Wellora.Areas.Admin.ViewModels;
using AdminEntity = Wellora.Areas.Admin.Models.Admin;





namespace Wellora.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAccountController : Controller
    {

        private readonly ApplicationDbContext _context;

        public AdminAccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        /*
         * [HttpGet]
        public IActionResult AdminRegistration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminRegistration(AdminRegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                // Check if username already exists
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "This username is already taken.");
                    return View(model);
                }

                // Create User
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Username = model.Username, // new field
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "Admin",
                    Status = "active", // default enum value
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // commit so user_id is generated

                // Create Admin linked to User
                var Admin = new AdminEntity
                {
                    UserId = user.UserId, // foreign key
                    FullName = $"{model.FirstName} {model.LastName}",
                    DateOfBirth = new DateOnly(1900, 1, 1), // temporary placeholder
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow

                };

                _context.Admins.Add(Admin);
                await _context.SaveChangesAsync();

                return RedirectToAction("AdminLogin", "Account", new { area = "Admin" });
            }

            return View(model);
        }
        */




        public IActionResult AdminLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.LoginIdentifier || u.Username == model.LoginIdentifier);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid login credentials.");
                return View(model);
            }

            // Build claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Sign in
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true, // keep logged in across browser sessions
                    ExpiresUtc = DateTime.UtcNow.AddHours(2)
                });

            // Redirect to dashboard
            return RedirectToAction("AIChat", "Admin", new { area = "Admin" });
        }

        [HttpGet]
        public IActionResult CheckUsername(string username)
        {
            bool exists = _context.Users.Any(u => u.Username == username);
            return Json(new { available = !exists });
        }


        public IActionResult AIChat()
        { return View(); }
    }
}
