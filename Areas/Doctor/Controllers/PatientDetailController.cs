using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Wellora.Areas.Patient.Models;
using Wellora.Data;
using Wellora.Models;
using Wellora.ViewModels.PatientDetail;

namespace Wellora.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize (Roles = "doctor")]
    public class PatientDetailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientDetailController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> PatientDetail(int patientId, int appointmentId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            var currentAppointment = await _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            var previousAppointments = await _context.Appointments
                .Where(a => a.PatientId == patientId && a.AppointmentDate < currentAppointment.AppointmentDate)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(10)
                .Select(a => new AppointmentSummaryViewModel
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    PaymentStatus = a.PaymentStatus
                })
                .ToListAsync();

            var totalAppointments = await _context.Appointments.CountAsync(a => a.PatientId == patientId);

            var lastAppointment = await _context.Appointments
                .Where(a => a.PatientId == patientId && a.AppointmentDate < currentAppointment.AppointmentDate)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentSummaryViewModel
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    PaymentStatus = a.PaymentStatus
                })
                .FirstOrDefaultAsync();

            var vm = new PatientDetailViewModel
            {
                Patient = patient,
                CurrentAppointment = currentAppointment,
                PreviousAppointments = previousAppointments,
                Age = CalculateAge(patient.DateOfBirth),
                TotalAppointments = totalAppointments,
                LastAppointment = lastAppointment
            };

            // 👉 Add this line here
            if (lastAppointment != null)
            {
                vm.LastAppointmentTimePassed = CalculateTimePassed(lastAppointment.AppointmentDate);
            }

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> SaveNote(NoteViewModel model)
        {
            var appointment = await _context.Appointments.FindAsync(model.AppointmentId);
            if (appointment != null)
            {
                appointment.Notes = model.Note;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("PatientDetail", new { patientId = appointment.PatientId, appointmentId = appointment.AppointmentId });
        }

        private string CalculateAge(DateOnly dob)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            int years = today.Year - dob.Year;
            int months = today.Month - dob.Month;
            int days = today.Day - dob.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(today.Year, today.Month - 1);
            }
            if (months < 0)
            {
                years--;
                months += 12;
            }

            return $"{years} years {months} months {days} days";
        }

        private string CalculateTimePassed(DateTime appointmentDate)
        {
            var now = DateTime.Now;
            int years = now.Year - appointmentDate.Year;
            int months = now.Month - appointmentDate.Month;
            int days = now.Day - appointmentDate.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(now.Year, now.Month == 1 ? 12 : now.Month - 1);
            }
            if (months < 0)
            {
                years--;
                months += 12;
            }

            string result = "";
            if (years > 0) result += $"{years} years ";
            if (months > 0) result += $"{months} months ";
            if (days > 0) result += $"{days} days ";

            return string.IsNullOrEmpty(result) ? "Today" : result.Trim() + " ago";
        }

    }
}
