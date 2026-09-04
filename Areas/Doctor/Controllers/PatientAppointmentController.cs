using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Wellora.Areas.Doctor.ViewModels.PatientAppointment;
using Wellora.Data;

namespace Wellora.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "doctor")]
    public class PatientAppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientAppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // PATIENT APPOINTMENT LIST
        // =========================================================

        public async Task<IActionResult> PatientAppointmentList(
            string? gender,
            string? search,
            int page = 1)
        {
            const int pageSize = 20;

            // -----------------------------------------------------
            // Make sure page is valid
            // -----------------------------------------------------

            if (page < 1)
            {
                page = 1;
            }

            // -----------------------------------------------------
            // GET CURRENT DOCTOR ID
            // -----------------------------------------------------

            var doctorIdClaim = User.FindFirstValue("CurrentDoctorId");

            if (!int.TryParse(doctorIdClaim, out int doctorId))
            {
                return Unauthorized();
            }

            // -----------------------------------------------------
            // DATE RANGE
            // -----------------------------------------------------

            var today = DateTime.Today;

            // Show appointments from today
            // up to 7 months from today
            var maxDate = today.AddMonths(7);

            // -----------------------------------------------------
            // BASE QUERY
            // -----------------------------------------------------
            //
            // IMPORTANT:
            // Only appointments belonging to the currently
            // logged-in doctor are returned.
            //
            // Cancelled appointments are excluded because this
            // page is showing incoming/upcoming appointments.
            // -----------------------------------------------------

            var query = _context.Appointments
                .Include(a => a.Patient)
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate >= today &&
                    a.AppointmentDate <= maxDate &&
                    a.Status != "cancelled");

            // -----------------------------------------------------
            // SEARCH BY PATIENT NAME
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(a =>
                    a.Patient.FullName.Contains(search));
            }

            // -----------------------------------------------------
            // GENDER FILTER
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(gender))
            {
                query = query.Where(a =>
                    a.Patient.Gender == gender);
            }

            // -----------------------------------------------------
            // TOTAL COUNT
            // -----------------------------------------------------

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize);

            // -----------------------------------------------------
            // IF REQUESTED PAGE DOES NOT EXIST
            // -----------------------------------------------------

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // -----------------------------------------------------
            // GET APPOINTMENTS
            // -----------------------------------------------------
            //
            // Earliest appointment first:
            //
            // Today
            // Tomorrow
            // Next day
            // etc.
            //
            // -----------------------------------------------------

            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentId)

                .Skip((page - 1) * pageSize)
                .Take(pageSize)

                .Select(a => new PatientAppointmentListViewModel
                {
                    AppointmentId = a.AppointmentId,

                    PatientId = a.PatientId,

                    AppointmentDate = a.AppointmentDate,

                    Status = a.Status,

                    PatientName = a.Patient.FullName,

                    Gender = a.Patient.Gender,

                    Age =
                        DateTime.Today.Year -
                        a.Patient.DateOfBirth.Year -
                        (
                            DateTime.Today.DayOfYear <
                            a.Patient.DateOfBirth.DayOfYear
                                ? 1
                                : 0
                        ),

                    ProfilePhoto = a.Patient.ProfilePhoto,

                    // -------------------------------------------------
                    // Find patient's previous appointment
                    // -------------------------------------------------

                    LastVisitedDate = _context.Appointments
                        .Where(x =>
                            x.PatientId == a.PatientId &&
                            x.AppointmentDate < a.AppointmentDate &&
                            x.Status != "cancelled")
                        .OrderByDescending(x => x.AppointmentDate)
                        .Select(x => (DateTime?)x.AppointmentDate)
                        .FirstOrDefault()
                })

                .ToListAsync();

            // -----------------------------------------------------
            // PAGINATION INFORMATION
            // -----------------------------------------------------

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            // -----------------------------------------------------
            // KEEP FILTER VALUES
            // -----------------------------------------------------
            //
            // These are needed when the doctor moves between
            // pagination pages.
            //
            // -----------------------------------------------------

            ViewBag.Gender = gender;
            ViewBag.Search = search;

            // -----------------------------------------------------
            // HTMX REQUEST
            // -----------------------------------------------------
            //
            // When search/filter/pagination is triggered through
            // HTMX, return only the partial.
            //
            // -----------------------------------------------------

            if (Request.Headers["HX-Request"] == "true")
            {
                return PartialView(
                    "_AppointmentsPartial",
                    appointments);
            }

            // -----------------------------------------------------
            // NORMAL PAGE REQUEST
            // -----------------------------------------------------

            return View(appointments);
        }
    }
}
