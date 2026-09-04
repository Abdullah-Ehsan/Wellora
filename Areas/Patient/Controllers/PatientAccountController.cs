using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using System.Security.Claims;
using Wellora.Areas.Patient.Models;
using Wellora.Areas.Patient.ViewModels.PatientAccount;
using Wellora.Data;
using Wellora.Models;
using PatientEntity = Wellora.Areas.Patient.Models.Patient;



namespace Wellora.Areas.Patient.Controllers
{
    [Area("Patient")]
    public class PatientAccountController : Controller
    {
        
        private readonly ApplicationDbContext _context;

        public PatientAccountController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult AccountBanned(string role = "patient")
        {
            ViewBag.Role = role;
            return View();
        }


        [HttpGet]
        public IActionResult PatientRegistration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PatientRegistration(PatientRegistrationViewModel model)
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
                    Role = "patient",
                    Status = "active",
                    AccountSituation = "no_banned",// default enum value
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // commit so user_id is generated

                // Create Patient linked to User
                var patient = new PatientEntity
                {
                    UserId = user.UserId, // foreign key
                    FullName = $"{model.FirstName} {model.LastName}",
                    DateOfBirth = new DateOnly(1900, 1, 1), // temporary placeholder
                    Gender = "other",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                    // other fields left null for now
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                return RedirectToAction("PatientLogin", "PatientAccount", new { area = "Patient" });
            }

            return View(model);
        }



        //for logging out
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PatientLogout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(
                "PatientLogin",
                "PatientAccount",
                new { area = "Patient" }
            );
        }

        //logging in
        public IActionResult PatientLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PatientLogin(PatientLoginViewModel model)
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
                new Claim(ClaimTypes.Role, user.Role, user.Role.ToLower())
            };

            // Get patient profile
            var patient = await _context.Patients
                .SingleOrDefaultAsync(d => d.UserId == user.UserId);

            if (patient == null)
            {
                ModelState.AddModelError("", "Patient profile not found.");
                return View(model);
            }

            // Add AdminId claim
            claims.Add(
                new Claim("CurrentPatientId", patient.PatientId.ToString())
            );

            claims.Add(
                new Claim("ProfilePicturePath", patient.ProfilePhoto ?? "")
            );

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
            return RedirectToAction("PatientDashboard", "Patient", new { area = "Patient" });
        }

        [HttpGet]
        public IActionResult CheckUsername(string username)
        {
            bool exists = _context.Users.Any(u => u.Username == username);
            return Json(new { available = !exists });
        }


       
    }
}
