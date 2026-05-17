using BCrypt.Net;
using Org.BouncyCastle.Crypto.Generators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wellora.Models;
using Wellora.Data;
using Wellora.Areas.Doctor.Models;
using Wellora.Areas.Doctor.ViewModels;
using DoctorEntity = Wellora.Areas.Doctor.Models.Doctor;





namespace Wellora.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class DoctorAccountController : Controller
    {

        private readonly ApplicationDbContext _context;

        public DoctorAccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult DoctorRegistration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DoctorRegistration(DoctorRegistrationViewModel model)
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
                    Username = model.Username, 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "doctor",
                    Status = "active", 
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // commit so user_id is generated

                // Create Doctor linked to User
                var Doctor = new DoctorEntity
                {
                    UserId = user.UserId, // foreign key
                    FullName = $"{model.FirstName} {model.LastName}",
                    DateOfBirth = new DateOnly(1900, 1, 1), 
                    Gender = "other", 
                    LicenseNumber = "0000000000", 
                    Specialization = "General", 
                    ConsultationFee = 0.00m, 
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,

                };

                _context.Doctors.Add(Doctor);
                await _context.SaveChangesAsync();

                return RedirectToAction("DoctorLogin", "Account", new { area = "Doctor" });
            }

            return View(model);
        }




        public IActionResult DoctorLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoctorLogin(DoctorLoginViewModel model)
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
            return RedirectToAction("AIChat", "Doctor", new { area = "Doctor" });
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
