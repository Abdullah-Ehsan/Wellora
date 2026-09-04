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
using AdminEntity = Wellora.Areas.Admin.Models.Admin;
using Wellora.Areas.Admin.ViewModels.AdminAccount;





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

        [HttpGet]
        public IActionResult AccountBanned(string role = "patient")
        {
            ViewBag.Role = role;
            return View();
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


        //for logging out
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(
                "AdminLogin",
                "AdminAccount",
                new { area = "Admin" }
            );
        }

        //for logging in
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

            // Verify account situation
            if (user.AccountSituation == "banned")
            {
                ViewBag.Role = user.Role;
                return View("AccountBanned");
            }

            // Build claims
            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Get admin profile
            var admin = await _context.Admins
                .SingleOrDefaultAsync(d => d.UserId == user.UserId);

            if (admin == null)
            {
                ModelState.AddModelError("", "Admin profile not found.");
                return View(model);
            }

            // Add AdminId claim
            claims.Add(
                new Claim("CurrentAdminId", admin.AdminId.ToString())
            );

            // Add Admin Seniority claim
            claims.Add(
                new Claim("CurrentAdminSeniority", admin.Seniority ?? "junior")
            );

            // Add Profile Picture claim
            claims.Add(
                new Claim("ProfilePicturePath", admin.ProfilePicture ?? "")
            );

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            // Sign in
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(2)
                });


            // Redirect to dashboard
            return RedirectToAction("AdminDashboard", "AdminDashboard", new { area = "Admin" });
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
