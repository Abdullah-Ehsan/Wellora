using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wellora.Areas.Patient.Services.Scheduling;
using Wellora.Areas.Patient.ViewModels.MakeAppointment;
using Wellora.Data;
using Wellora.Models;

namespace Wellora.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "patient")]
    

    public class MakeAppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AppointmentSlotService _slotService;
        private readonly PatientAppointmentService _appointmentService;

        public MakeAppointmentController(
            ApplicationDbContext context,
            AppointmentSlotService slotService,
            PatientAppointmentService appointmentService)
        {
            _context = context;
            _slotService = slotService;
            _appointmentService = appointmentService;
        }

        // =========================
        // BOOKING PAGE
        // =========================
        public IActionResult AppointmentBooking(int doctorId, int? year, int? month)
        {
            var doctor = _context.Doctors
                .FirstOrDefault(d => d.DoctorId == doctorId);

            if (doctor == null) return NotFound();

            var targetYear = year ?? DateTime.Today.Year;
            var targetMonth = month ?? DateTime.Today.Month;

            ViewBag.Year = targetYear;
            ViewBag.Month = targetMonth;

            var availableDates = GenerateAvailableDates(doctorId);

            var vm = new AppointmentBookingViewModel
            {
                DoctorId = doctor.DoctorId,
                DoctorName = doctor.FullName,
                Specialization = doctor.Specialization,
                SubSpecialization = doctor.SubSpecialties,
                ProfilePhoto = doctor.ProfilePhoto,
                ConsultationFee = doctor.ConsultationFee,

                AvailableDates = availableDates,
                CalendarCells = BuildCalendarCells(availableDates, targetYear, targetMonth)
            };

            return View("AppointmentBooking", vm);
        }

        // =========================
        // GET SLOTS (AJAX or reload)
        // =========================
        [HttpGet("/Patient/MakeAppointment/GetAvailableSlots")]
        public IActionResult GetSlots(int doctorId, DateTime date)
        {
            var result = _slotService.GenerateSlots(doctorId, date);

            return Json(new
            {
                morningSlots = result.Morning,
                afternoonSlots = result.Afternoon,
                eveningSlots = result.Evening,
                noSlots = result.NoSlots
            });
        }

        // =========================
        // CONFIRM BOOKING
        // =========================
        [HttpPost]
        public IActionResult Confirm(AppointmentBookingViewModel vm)
        {
            if (vm.SelectedDate == null || string.IsNullOrEmpty(vm.SelectedSlot))
            {
                ModelState.AddModelError("", "Please select date and slot.");
                return RedirectToAction("AppointmentBooking", new
                {
                    doctorId = vm.DoctorId
                });
            }

            // rebuild full datetime
            var slotDateTime = DateTime.Parse($"{vm.SelectedDate:yyyy-MM-dd} {vm.SelectedSlot}");

            // DOUBLE BOOKING PROTECTION
            var exists = _context.Appointments.Any(a =>
                a.DoctorId == vm.DoctorId &&
                a.AppointmentDate == slotDateTime &&
                a.Status != "cancelled");

            if (exists)
            {
                TempData["Error"] = "This slot is already booked.";
                return RedirectToAction("AppointmentBooking", new
                {
                    doctorId = vm.DoctorId
                });
            }

            // get patient
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId);

            var patientId = _context.Patients
                .Where(p => p.UserId == userId)
                .Select(p => p.PatientId)
                .FirstOrDefault();

            if (patientId == 0)
                return Unauthorized();

            var appointment = new Appointment
            {
                DoctorId = vm.DoctorId,
                PatientId = patientId,
                AppointmentDate = slotDateTime,
                Status = "scheduled",
                PaymentStatus = "pending",
                PaymentMethod = vm.PaymentMethod,
                ConsultationFee = vm.ConsultationFee,
                Notes = vm.Notes,
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            return RedirectToAction("PatientAppointment");
        }

        // =========================
        // AVAILABLE DATES (6 MONTH RULE)
        // =========================
        private List<DateTime> GenerateAvailableDates(int doctorId)
        {
            var today = DateTime.Today;
            var end = today.AddMonths(6);

            var schedules = _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && s.IsActive)
                .ToList();

            var result = new List<DateTime>();

            for (var d = today; d <= end; d = d.AddDays(1))
            {
                int dbDay = (int)d.DayOfWeek == 0 ? 7 : (int)d.DayOfWeek;

                if (schedules.Any(s => s.DayOfWeek == dbDay))
                    result.Add(d);
            }

            return result;
        }

        // =========================
        // CALENDAR BUILDER
        // =========================
        private List<CalendarCell> BuildCalendarCells(
            List<DateTime> availableDates,
            int year,
            int month)
        {
            var cells = new List<CalendarCell>();

            var start = new DateTime(year, month, 1);
            var days = DateTime.DaysInMonth(year, month);

            var offset = ((int)start.DayOfWeek + 6) % 7; // Monday=0
            int day = 1;

            for (int i = 0; i < 42; i++) // 6x7 grid
            {
                if (i < offset || day > days)
                {
                    cells.Add(new CalendarCell { Date = null });
                }
                else
                {
                    var date = new DateTime(year, month, day);

                    cells.Add(new CalendarCell
                    {
                        Date = date,
                        IsAvailable = availableDates.Contains(date)
                    });

                    day++;
                }
            }

            return cells;
        }

        [HttpGet]
        public IActionResult GetCalendar(int doctorId, int year, int month)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == doctorId);
            if (doctor == null) return NotFound();

            var availableDates = GenerateAvailableDates(doctor.DoctorId);
            var cells = BuildCalendarCells(availableDates, year, month);

            return PartialView("_CalendarPartial", cells);
        }








        //this the second page of this controller

        [HttpGet]
        public IActionResult PatientAppointment()
        {
            return View();
        }

        // Secure AJAX JSON Endpoint calling the external Service Layer
        [HttpGet("/Patient/MakeAppointment/GetUpcomingAppointments")]
        public IActionResult GetUpcomingAppointments(string sortBy = "nearest", string feeSort = "", string timeSlot = "")
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var patientId = _context.Patients
                .Where(p => p.UserId == userId)
                .Select(p => p.PatientId)
                .FirstOrDefault();

            if (patientId == 0) return PartialView("_AppointmentTableRows", new List<PatientAppointmentItem>());

            // Query data via service layer
            var dataMatrix = _appointmentService.GetFilteredAppointmentsForPatient(patientId, sortBy, feeSort, timeSlot);

            // Render partial view directly to HTML string stream response
            return PartialView("_AppointmentTableRows", dataMatrix);
        }


        //Page 3 this is only view the ticket for the appointment

        // Add this method inside your existing MakeAppointmentController class
        [HttpGet]
        public IActionResult ViewTicket(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = int.Parse(userIdClaim);

            // Get patient context ID
            var patientId = _context.Patients
                .Where(p => p.UserId == userId)
                .Select(p => p.PatientId)
                .FirstOrDefault();

            if (patientId == 0) return RedirectToAction("PatientAppointments");

            // Fetch unified record details using our secure method
            var ticketDetails = _appointmentService.GetAppointmentTicketDetails(id, patientId);
            if (ticketDetails == null) return NotFound();

            return View(ticketDetails);
        }
    }
}