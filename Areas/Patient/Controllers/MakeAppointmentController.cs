using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wellora.Areas.Patient.ViewModels;
using Wellora.Data;
using Wellora.Models;
using Wellora.Areas.Doctor.Models;
using DoctorEntity = Wellora.Areas.Doctor.Models.Doctor;

namespace Wellora.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "patient")]
    public class MakeAppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MakeAppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Appointment/Book/{doctorId}
        public IActionResult AppointmentBooking(int doctorId)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == doctorId);
            if (doctor == null) return NotFound();

            var vm = new AppointmentBookingViewModel
            {
                DoctorId = doctor.DoctorId,
                DoctorName = doctor.FullName,
                Specialization = doctor.Specialization,
                SubSpecialization = doctor.SubSpecialties,
                ProfilePhoto = doctor.ProfilePhoto,
                ConsultationFee = doctor.ConsultationFee,
                AvailableDates = GenerateAvailableDates(doctor)
            };

            return View("AppointmentBooking", vm);
        }

        [HttpGet]
        public IActionResult GetAvailableDates(int doctorId, int year, int month)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == doctorId);
            if (doctor == null) return NotFound();

            var dates = GenerateAvailableDates(doctor)
                .Where(d => d.Year == year && d.Month == month)
                .Select(d => d.ToString("yyyy-MM-dd"))
                .ToList();

            return Json(dates);
        }


        // GET: Appointment/GetSlots
        [HttpGet]
        public IActionResult GetAvailableSlots(int doctorId, DateTime date)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == doctorId);
            if (doctor == null) return NotFound();

            var slots = GenerateSlots(doctor, date);

            // Remove already booked slots
            var booked = _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date && a.Status != "cancelled")
                .Select(a => a.AppointmentDate.ToString("hh:mm tt")) // AM/PM format
                .ToList();

            slots.MorningSlots = slots.MorningSlots.Except(booked).ToList();
            slots.AfternoonSlots = slots.AfternoonSlots.Except(booked).ToList();
            slots.EveningSlots = slots.EveningSlots.Except(booked).ToList();

            return Json(slots);
        }

        // POST: Appointment/Confirm
        [HttpPost]
        public IActionResult Confirm(AppointmentBookingViewModel vm)
        {
            if (!ModelState.IsValid || vm.SelectedDate == null || string.IsNullOrEmpty(vm.SelectedSlot))
            {
                return View("AppointmentBooking", vm);
            }

            // Check if slot is free
            var slotDateTime = DateTime.Parse($"{vm.SelectedDate.Value.ToShortDateString()} {vm.SelectedSlot}");
            var exists = _context.Appointments.Any(a =>
                a.DoctorId == vm.DoctorId &&
                a.AppointmentDate == slotDateTime &&
                a.Status != "cancelled");

            if (exists)
            {
                ModelState.AddModelError("", "This slot is already booked.");
                return View("AppointmentBooking", vm);
            }

            var appointment = new Appointment
            {
                DoctorId = vm.DoctorId,
                PatientId = 1, // replace with logged-in patient ID
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

        public IActionResult PatientAppointment()
        {
            return View();
        }

        // ---------------- Helper Methods ----------------

        private List<DateTime> GenerateAvailableDates(DoctorEntity doctor)
        {
            var availableDates = new List<DateTime>();
            var today = DateTime.Today;
            var endDate = today.AddMonths(6);

            var daysAvailable = doctor.DaysAvailable?.Split('_') ?? Array.Empty<string>();

            for (var date = today; date <= endDate; date = date.AddDays(1))
            {
                var dayName = date.ToString("ddd"); // Mon, Tue, etc.
                if (daysAvailable.Any(d => dayName.StartsWith(d, StringComparison.OrdinalIgnoreCase)))
                {
                    availableDates.Add(date);
                }
            }

            return availableDates;
        }

        private AppointmentBookingViewModel GenerateSlots(DoctorEntity doctor, DateTime date)
        {
            var vm = new AppointmentBookingViewModel();

            var duration = doctor.AppointmentDurationMin ?? 30;
            var clinicRanges = doctor.ClinicHours?.Split(',') ?? Array.Empty<string>();
            var breakRanges = doctor.BreakTimes?.Split(',') ?? Array.Empty<string>();

            var morning = new List<string>();
            var afternoon = new List<string>();
            var evening = new List<string>();

            foreach (var range in clinicRanges)
            {
                var times = range.Split('-');
                if (times.Length != 2) continue;

                var start = DateTime.Parse(times[0]);
                var end = DateTime.Parse(times[1]);

                for (var slot = start; slot < end; slot = slot.AddMinutes(duration))
                {
                    // Skip breaks
                    if (breakRanges.Any(br =>
                    {
                        var brTimes = br.Split('-');
                        if (brTimes.Length != 2) return false;
                        var brStart = DateTime.Parse(brTimes[0]);
                        var brEnd = DateTime.Parse(brTimes[1]);
                        return slot >= brStart && slot < brEnd;
                    }))
                    {
                        continue;
                    }

                    var formatted = slot.ToString("hh:mm tt"); // AM/PM
                    if (slot.Hour < 12) morning.Add(formatted);
                    else if (slot.Hour < 17) afternoon.Add(formatted);
                    else evening.Add(formatted);
                }
            }

            vm.MorningSlots = morning;
            vm.AfternoonSlots = afternoon;
            vm.EveningSlots = evening;

            return vm;
        }

    }
}