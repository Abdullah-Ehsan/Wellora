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
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            var currentAppointment = await _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == appointmentId &&
                    a.PatientId == patientId);

            if (currentAppointment == null)
            {
                return NotFound("Appointment not found.");
            }

            // ---------------------------------------------------------
            // Previous appointments
            // ---------------------------------------------------------

            var previousAppointments = await _context.Appointments
                .Where(a =>
                    a.PatientId == patientId &&
                    a.AppointmentDate < currentAppointment.AppointmentDate)
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

            // ---------------------------------------------------------
            // Total appointments
            // ---------------------------------------------------------

            var totalAppointments = await _context.Appointments
                .CountAsync(a => a.PatientId == patientId);

            // ---------------------------------------------------------
            // Last appointment
            // ---------------------------------------------------------

            var lastAppointment = await _context.Appointments
                .Where(a =>
                    a.PatientId == patientId &&
                    a.AppointmentDate < currentAppointment.AppointmentDate)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentSummaryViewModel
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    PaymentStatus = a.PaymentStatus
                })
                .FirstOrDefaultAsync();

            // ---------------------------------------------------------
            // PRIMARY DOCTOR
            // ---------------------------------------------------------

            var primaryDoctor = new PrimaryDoctorViewModel
            {
                Exists = false
            };

            // First check patient's primary_doctor_id
            if (patient.PrimaryDoctorId.HasValue)
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d =>
                        d.DoctorId == patient.PrimaryDoctorId.Value);

                if (doctor != null)
                {
                    primaryDoctor.Exists = true;
                    primaryDoctor.IsOutsideDoctor = false;
                    primaryDoctor.DoctorName = doctor.FullName;
                    primaryDoctor.Specialty = doctor.Specialization;
                    primaryDoctor.ContactNumber = doctor.ContactNumber;
                    primaryDoctor.ProfilePhoto = doctor.ProfilePhoto;
                    primaryDoctor.HospitalAddress = doctor.HospitalAddress;
                }
            }

            // If no normal primary doctor was found,
            // look for an outside doctor.
            if (!primaryDoctor.Exists)
            {
                var outsideDoctor = await _context.PatientOutsideDoctors
                    .Include(x => x.OutsideDoctor)
                    .Where(x => x.PatientId == patientId)
                    .Select(x => x.OutsideDoctor)
                    .FirstOrDefaultAsync();

                if (outsideDoctor != null)
                {
                    primaryDoctor.Exists = true;
                    primaryDoctor.IsOutsideDoctor = true;
                    primaryDoctor.DoctorName = outsideDoctor.DoctorName;
                    primaryDoctor.Specialty = outsideDoctor.DoctorSpecialty;
                    primaryDoctor.ContactNumber = outsideDoctor.DoctorPhone;
                    primaryDoctor.ProfilePhoto = outsideDoctor.DoctorPhoto;
                    primaryDoctor.HospitalName = outsideDoctor.HospitalName;

                    // OutsideDoctor doesn't have HospitalAddress.
                    // We combine city and country for the location.
                    primaryDoctor.HospitalAddress =
                        string.Join(", ",
                            new[]
                            {
                        outsideDoctor.HospitalCity,
                        outsideDoctor.HospitalCountry
                            }
                            .Where(x => !string.IsNullOrWhiteSpace(x)));
                }
            }

            // ---------------------------------------------------------
            // APPOINTMENT ACTIONS
            // ---------------------------------------------------------

            var now = DateTime.Now;

            var appointmentTimeDifference =
                currentAppointment.AppointmentDate - now;

            var appointmentActions = new AppointmentActionsViewModel
            {
                ScheduledStatus = currentAppointment.Status,
                PaymentStatus = currentAppointment.PaymentStatus,

                // Confirm is only available while appointment is scheduled
                CanConfirm =
                    currentAppointment.Status == "scheduled",

                // Cancel only if appointment is scheduled
                // AND at least 7 days away
                CanCancel =
                    currentAppointment.Status == "scheduled" &&
                    appointmentTimeDifference.TotalDays >= 7,

                // Payment can only be changed when currently pending
                // and appointment is today or yesterday
                CanMarkPaid =
                    currentAppointment.PaymentStatus == "pending" &&
                    currentAppointment.AppointmentDate.Date >= now.Date.AddDays(-1) &&
                    currentAppointment.AppointmentDate.Date <= now.Date
            };

            // ---------------------------------------------------------
            // BUILD VIEW MODEL
            // ---------------------------------------------------------

            var vm = new PatientDetailViewModel
            {
                Patient = patient,
                CurrentAppointment = currentAppointment,
                PreviousAppointments = previousAppointments,
                Age = CalculateAge(patient.DateOfBirth),
                TotalAppointments = totalAppointments,
                LastAppointment = lastAppointment,

                PrimaryDoctor = primaryDoctor,
                AppointmentActions = appointmentActions
            };

            if (lastAppointment != null)
            {
                vm.LastAppointmentTimePassed =
                    CalculateTimePassed(lastAppointment.AppointmentDate);
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



        //============================
        //buttons actions

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAppointment(int appointmentId, int patientId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == appointmentId &&
                    a.PatientId == patientId);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            // Only scheduled appointments can be confirmed
            if (appointment.Status != "scheduled")
            {
                TempData["ErrorMessage"] = "This appointment cannot be confirmed.";

                return RedirectToAction("PatientDetail", new
                {
                    patientId = patientId,
                    appointmentId = appointmentId
                });
            }

            // scheduled → checked
            appointment.Status = "checked";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment confirmed successfully.";

            return RedirectToAction("PatientDetail", new
            {
                patientId = patientId,
                appointmentId = appointmentId
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int appointmentId, int patientId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == appointmentId &&
                    a.PatientId == patientId);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            // Only scheduled appointments can be cancelled
            if (appointment.Status != "scheduled")
            {
                TempData["ErrorMessage"] = "This appointment cannot be cancelled.";

                return RedirectToAction("PatientDetail", new
                {
                    patientId = patientId,
                    appointmentId = appointmentId
                });
            }

            var now = DateTime.Now;

            var timeUntilAppointment =
                appointment.AppointmentDate - now;

            // Cancellation requires at least 7 complete days
            if (timeUntilAppointment.TotalDays < 7)
            {
                TempData["ErrorMessage"] =
                    "This appointment cannot be cancelled because it is less than 7 days away.";

                return RedirectToAction("PatientDetail", new
                {
                    patientId = patientId,
                    appointmentId = appointmentId
                });
            }

            // scheduled → cancelled
            appointment.Status = "cancelled";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment cancelled successfully.";

            return RedirectToAction("PatientDetail", new
            {
                patientId = patientId,
                appointmentId = appointmentId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaymentPaid(int appointmentId, int patientId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == appointmentId &&
                    a.PatientId == patientId);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            // Only pending payments can be changed
            if (appointment.PaymentStatus != "pending")
            {
                TempData["ErrorMessage"] =
                    "This payment cannot be changed.";

                return RedirectToAction("PatientDetail", new
                {
                    patientId = patientId,
                    appointmentId = appointmentId
                });
            }

            var today = DateTime.Now.Date;
            var appointmentDate = appointment.AppointmentDate.Date;

            // Payment can be marked as paid only:
            // - on appointment day
            // - or the following day
            var canMarkPaid =
                appointmentDate >= today.AddDays(-1) &&
                appointmentDate <= today;

            if (!canMarkPaid)
            {
                TempData["ErrorMessage"] =
                    "Payment can only be marked as paid on the appointment day or the following day.";

                return RedirectToAction("PatientDetail", new
                {
                    patientId = patientId,
                    appointmentId = appointmentId
                });
            }

            // pending → paid
            appointment.PaymentStatus = "paid";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment marked as paid successfully.";

            return RedirectToAction("PatientDetail", new
            {
                patientId = patientId,
                appointmentId = appointmentId
            });
        }

    }
}
