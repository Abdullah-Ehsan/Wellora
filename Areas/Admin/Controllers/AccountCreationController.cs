using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Cryptography;

using Wellora.Areas.Admin.ViewModels.AccountCreation;
using Wellora.Data;
using Wellora.Models;

using AdminEntity = Wellora.Areas.Admin.Models.Admin;
using DoctorEntity = Wellora.Areas.Doctor.Models.Doctor;
using PatientEntity = Wellora.Areas.Patient.Models.Patient;

namespace Wellora.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class AccountCreationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountCreationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // ACCOUNT CREATION PAGE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new AccountCreationViewModel();

            await SetHeadAdminViewBagAsync();

            return View(model);
        }


        // =========================================================
        // CREATE DOCTOR / PATIENT ACCOUNT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserAccount(
            [Bind(Prefix = "UserAccount")]
            CreateUserAccountViewModel model)
        {
            // =====================================================
            // VALIDATE ROLE
            // =====================================================

            var allowedRoles = new[]
            {
                "doctor",
                "patient"
            };

            if (string.IsNullOrWhiteSpace(model.Role) ||
                !allowedRoles.Contains(
                    model.Role,
                    StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Please select a valid role."
                );
            }


            // =====================================================
            // MODEL VALIDATION
            // =====================================================

            if (!ModelState.IsValid)
            {
                await SetHeadAdminViewBagAsync();

                return View(
                    "Index",
                    new AccountCreationViewModel
                    {
                        UserAccount = model
                    }
                );
            }


            // =====================================================
            // NORMALIZE VALUES
            // =====================================================

            model.Role = model.Role!.Trim().ToLowerInvariant();

            model.Email = model.Email!.Trim();

            model.Username = model.Username!.Trim();


            // =====================================================
            // CHECK EMAIL
            // =====================================================

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email is already registered."
                );

                await SetHeadAdminViewBagAsync();

                return View(
                    "Index",
                    new AccountCreationViewModel
                    {
                        UserAccount = model
                    }
                );
            }


            // =====================================================
            // CHECK USERNAME
            // =====================================================

            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username == model.Username);

            if (usernameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Username),
                    "This username is already taken."
                );

                await SetHeadAdminViewBagAsync();

                return View(
                    "Index",
                    new AccountCreationViewModel
                    {
                        UserAccount = model
                    }
                );
            }


            // =====================================================
            // SAVE ORIGINAL PASSWORD
            // =====================================================
            // We need the original password for the PDF.
            // The database only receives the BCrypt hash.

            var accountPassword = model.Password;


            // =====================================================
            // CREATE USER
            // =====================================================

            var user = new User
            {
                FirstName = model.FirstName!.Trim(),
                LastName = model.LastName!.Trim(),

                Email = model.Email,
                Username = model.Username,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        model.Password
                    ),

                Role = model.Role,
                Status = "active",

                AccountSituation = "no_banned",

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();


            // =====================================================
            // CREATE DOCTOR
            // =====================================================

            if (model.Role == "doctor")
            {
                var doctor = new DoctorEntity
                {
                    UserId = user.UserId,

                    FullName =
                        $"{user.FirstName} {user.LastName}",

                    // Temporary defaults.
                    DateOfBirth =
                        new DateOnly(1900, 1, 1),

                    Gender = "other",

                    ConsultationFee = 0.00m,

                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Doctors.Add(doctor);

                await _context.SaveChangesAsync();


                // =================================================
                // GENERATE DOCTOR PDF
                // =================================================

                var pdf = GenerateAccountPdf(
                    accountType: "Doctor",
                    accountId: doctor.DoctorId.ToString(),
                    firstName: user.FirstName,
                    lastName: user.LastName,
                    email: user.Email,
                    username: user.Username,
                    password: accountPassword
                );


                // =================================================
                // DOWNLOAD PDF
                // =================================================

                return File(
                    pdf,
                    "application/pdf",
                    $"Wellora-Doctor-{doctor.DoctorId}.pdf"
                );
            }


            // =====================================================
            // CREATE PATIENT
            // =====================================================

            if (model.Role == "patient")
            {
                var patient = new PatientEntity
                {
                    UserId = user.UserId,

                    FullName =
                        $"{user.FirstName} {user.LastName}",

                    // Temporary defaults.
                    DateOfBirth =
                        new DateOnly(1900, 1, 1),

                    Gender = "other",

                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Patients.Add(patient);

                await _context.SaveChangesAsync();


                // =================================================
                // GENERATE PATIENT PDF
                // =================================================

                var pdf = GenerateAccountPdf(
                    accountType: "Patient",
                    accountId: patient.PatientId.ToString(),
                    firstName: user.FirstName,
                    lastName: user.LastName,
                    email: user.Email,
                    username: user.Username,
                    password: accountPassword
                );


                // =================================================
                // DOWNLOAD PDF
                // =================================================

                return File(
                    pdf,
                    "application/pdf",
                    $"Wellora-Patient-{patient.PatientId}.pdf"
                );
            }


            // This should never be reached because the role
            // has already been validated.

            return BadRequest("Invalid account role.");
        }


        // =========================================================
        // CREATE ADMIN ACCOUNT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdminAccount(
            [Bind(Prefix = "AdminAccount")]
            CreateAdminAccountViewModel model)
        {
            // =====================================================
            // SECURITY CHECK
            // =====================================================
            // Only a HEAD admin can create another admin.

            var currentAdmin = await GetCurrentAdminAsync();

            if (currentAdmin == null ||
                !string.Equals(
                    currentAdmin.Seniority,
                    "head",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }


            // =====================================================
            // MODEL VALIDATION
            // =====================================================

            if (!ModelState.IsValid)
            {
                await SetHeadAdminViewBagAsync();

                return View(
                    "Index",
                    new AccountCreationViewModel
                    {
                        AdminAccount = model
                    }
                );
            }


            // =====================================================
            // NORMALIZE VALUES
            // =====================================================

            model.Email = model.Email!.Trim();

            model.Username = model.Username!.Trim();

            model.Seniority =
                model.Seniority.Trim().ToLowerInvariant();


            // =====================================================
            // VALIDATE SENIORITY
            // =====================================================

            var allowedSeniorities = new[]
            {
                "junior",
                "mid",
                "senior",
                "head"
            };

            if (!allowedSeniorities.Contains(
                    model.Seniority,
                    StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(model.Seniority),
                    "Please select a valid seniority."
                );

                await SetHeadAdminViewBagAsync();

                return View(
                    "Index",
                    new AccountCreationViewModel
                    {
                        AdminAccount = model
                    }
                );
            }


            // =====================================================
            // CHECK EMAIL
            // =====================================================

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email is already registered."
                );

                await SetHeadAdminViewBagAsync();

                return View(
                    "Index",
                    new AccountCreationViewModel
                    {
                        AdminAccount = model
                    }
                );
            }


            // =====================================================
            // CHECK USERNAME
            // =====================================================

            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username == model.Username);

            if (usernameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Username),
                    "This username is already taken."
                );

                await SetHeadAdminViewBagAsync();

                return View(
                    "Index",
                    new AccountCreationViewModel
                    {
                        AdminAccount = model
                    }
                );
            }


            // =====================================================
            // SAVE ORIGINAL PASSWORD
            // =====================================================

            var accountPassword = model.Password;


            // =====================================================
            // CREATE USER
            // =====================================================

            var user = new User
            {
                FirstName = model.FirstName!.Trim(),
                LastName = model.LastName!.Trim(),

                Email = model.Email,
                Username = model.Username,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        model.Password
                    ),

                Role = "admin",
                Status = "active",

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();


            // =====================================================
            // CREATE ADMIN PROFILE
            // =====================================================

            var admin = new AdminEntity
            {
                UserId = user.UserId,

                FullName =
                    $"{user.FirstName} {user.LastName}",

                Seniority = model.Seniority,

                Status = "active",

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Admins.Add(admin);

            await _context.SaveChangesAsync();


            // =====================================================
            // GENERATE ADMIN PDF
            // =====================================================

            var pdf = GenerateAccountPdf(
                accountType: "Administrator",
                accountId: admin.AdminId.ToString(),
                firstName: user.FirstName,
                lastName: user.LastName,
                email: user.Email,
                username: user.Username,
                password: accountPassword,
                seniority: admin.Seniority
            );


            // =====================================================
            // DOWNLOAD PDF
            // =====================================================

            return File(
                pdf,
                "application/pdf",
                $"Wellora-Admin-{admin.AdminId}.pdf"
            );
        }


        // =========================================================
        // GET CURRENT ADMIN
        // =========================================================

        private async Task<AdminEntity?> GetCurrentAdminAsync()
        {
            var currentAdminIdClaim =
                User.FindFirst("CurrentAdminId")?.Value;

            if (!int.TryParse(
                    currentAdminIdClaim,
                    out var currentAdminId))
            {
                return null;
            }

            return await _context.Admins
                .SingleOrDefaultAsync(
                    a => a.AdminId == currentAdminId
                );
        }


        // =========================================================
        // SET HEAD ADMIN VIEW BAG
        // =========================================================

        private async Task SetHeadAdminViewBagAsync()
        {
            var currentAdmin = await GetCurrentAdminAsync();

            ViewBag.IsHeadAdmin =
                currentAdmin != null &&
                string.Equals(
                    currentAdmin.Seniority,
                    "head",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        // =========================================================
        // GENERATE NEW PASSWORD
        // =========================================================

        [HttpGet]
        public IActionResult GenerateNewPassword()
        {
            var password = GeneratePassword(8);

            return Content(password);
        }


        // =========================================================
        // GENERATE SECURE PASSWORD
        // =========================================================

        private static string GeneratePassword(int length = 8)
        {
            if (length < 4)
            {
                throw new ArgumentException(
                    "Password length must be at least 4.",
                    nameof(length));
            }

            const string uppercase =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            const string lowercase =
                "abcdefghijklmnopqrstuvwxyz";

            const string numbers =
                "0123456789";

            const string special =
                "!@#$%^&*";

            const string allCharacters =
                uppercase +
                lowercase +
                numbers +
                special;


            // =====================================================
            // GUARANTEE REQUIRED CHARACTER TYPES
            // =====================================================

            var password = new List<char>
            {
                uppercase[
                    RandomNumberGenerator.GetInt32(
                        uppercase.Length)],

                lowercase[
                    RandomNumberGenerator.GetInt32(
                        lowercase.Length)],

                numbers[
                    RandomNumberGenerator.GetInt32(
                        numbers.Length)],

                special[
                    RandomNumberGenerator.GetInt32(
                        special.Length)]
            };


            // =====================================================
            // FILL REMAINING CHARACTERS
            // =====================================================

            while (password.Count < length)
            {
                password.Add(
                    allCharacters[
                        RandomNumberGenerator.GetInt32(
                            allCharacters.Length)
                    ]
                );
            }


            // =====================================================
            // SECURE SHUFFLE
            // =====================================================

            for (int i = password.Count - 1; i > 0; i--)
            {
                int j =
                    RandomNumberGenerator.GetInt32(i + 1);

                (password[i], password[j]) =
                    (password[j], password[i]);
            }


            return new string(password.ToArray());
        }


        // =========================================================
        // GENERATE ACCOUNT PDF
        // =========================================================

        private static byte[] GenerateAccountPdf(
            string accountType,
            string accountId,
            string firstName,
            string lastName,
            string email,
            string username,
            string password,
            string? seniority = null)
        {
            return Document
                .Create(container =>
                {
                    container.Page(page =>
                    {
                        // =================================================
                        // PAGE SETTINGS
                        // =================================================

                        page.Size(PageSizes.A4);

                        page.Margin(50);

                        page.DefaultTextStyle(
                            x => x
                                .FontSize(11)
                                .FontColor("#334155")
                        );


                        // =================================================
                        // HEADER
                        // =================================================

                        page.Header()
                            .Column(column =>
                            {
                                column.Item()
                                    .Text("WELLORA")
                                    .FontSize(26)
                                    .Bold()
                                    .FontColor("#2563EB");

                                column.Item()
                                    .PaddingTop(5)
                                    .Text("Account Credentials")
                                    .FontSize(14)
                                    .FontColor("#64748B");
                            });


                        // =================================================
                        // CONTENT
                        // =================================================

                        page.Content()
                            .PaddingTop(30)
                            .Column(column =>
                            {
                                column.Spacing(15);


                                // =========================================
                                // ACCOUNT TYPE
                                // =========================================

                                column.Item()
                                    .Background("#EFF6FF")
                                    .Border(1)
                                    .BorderColor("#BFDBFE")
                                    .Padding(15)
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Text("Account Type")
                                            .Bold()
                                            .FontColor("#475569");

                                        row.RelativeItem()
                                            .AlignRight()
                                            .Text(accountType)
                                            .Bold()
                                            .FontColor("#2563EB");
                                    });


                                // =========================================
                                // ACCOUNT ID
                                // =========================================

                                column.Item()
                                    .Row(row =>
                                    {
                                        row.ConstantItem(130)
                                            .Text("Account ID")
                                            .Bold();

                                        row.RelativeItem()
                                            .Text(accountId);
                                    });


                                // =========================================
                                // NAME
                                // =========================================

                                column.Item()
                                    .Row(row =>
                                    {
                                        row.ConstantItem(130)
                                            .Text("Name")
                                            .Bold();

                                        row.RelativeItem()
                                            .Text(
                                                $"{firstName} {lastName}"
                                            );
                                    });


                                // =========================================
                                // EMAIL
                                // =========================================

                                column.Item()
                                    .Row(row =>
                                    {
                                        row.ConstantItem(130)
                                            .Text("Email")
                                            .Bold();

                                        row.RelativeItem()
                                            .Text(email);
                                    });


                                // =========================================
                                // USERNAME
                                // =========================================

                                column.Item()
                                    .Row(row =>
                                    {
                                        row.ConstantItem(130)
                                            .Text("Username")
                                            .Bold();

                                        row.RelativeItem()
                                            .Text(username);
                                    });


                                // =========================================
                                // PASSWORD
                                // =========================================

                                column.Item()
                                    .Background("#F8FAFC")
                                    .Border(1)
                                    .BorderColor("#CBD5E1")
                                    .Padding(15)
                                    .Row(row =>
                                    {
                                        row.ConstantItem(130)
                                            .Text("Password")
                                            .Bold();

                                        row.RelativeItem()
                                            .Text(password)
                                            .Bold()
                                            .FontSize(13);
                                    });


                                // =========================================
                                // SENIORITY
                                // ADMIN ONLY
                                // =========================================

                                if (!string.IsNullOrWhiteSpace(seniority))
                                {
                                    column.Item()
                                        .Row(row =>
                                        {
                                            row.ConstantItem(130)
                                                .Text("Seniority")
                                                .Bold();

                                            row.RelativeItem()
                                                .Text(
                                                    seniority
                                                        .ToUpperInvariant()
                                                );
                                        });
                                }


                                // =========================================
                                // SECURITY NOTICE
                                // =========================================

                                column.Item()
                                    .PaddingTop(20)
                                    .Background("#FEF3C7")
                                    .Border(1)
                                    .BorderColor("#FDE68A")
                                    .Padding(15)
                                    .Text(
                                        "Keep these account credentials secure. " +
                                        "Do not share the password with " +
                                        "unauthorized persons."
                                    )
                                    .FontColor("#92400E");
                            });


                        // =================================================
                        // FOOTER
                        // =================================================

                        page.Footer()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span(
                                    "Wellora • Account Credentials • "
                                );

                                text.Span(
                                    DateTime.UtcNow
                                        .ToString("dd MMMM yyyy")
                                );
                            });
                    });
                })
                .GeneratePdf();
        }
    }
}
