using global::Wellora.Areas.Patient.ViewModels;
using global::Wellora.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Wellora.Areas.Patient.ViewModels.MakeAppointment;


namespace Wellora.Areas.Patient.Services.Scheduling
{
    public class PatientAppointmentService
    {
        private readonly ApplicationDbContext _context;

        public PatientAppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PatientAppointmentItem> GetFilteredAppointmentsForPatient(int patientId, string sortBy, string feeSort, string timeSlot)
        {
            // Query upcoming scheduled bookings linked explicitly to this PatientId
            var query = _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId && a.AppointmentDate >= DateTime.Now && a.Status == "scheduled")
                .AsQueryable();

            // 1. Time Slot Filtering Logic
            if (!string.IsNullOrEmpty(timeSlot))
            {
                if (timeSlot == "morning")
                {
                    query = query.Where(a => a.AppointmentDate.Hour >= 8 && a.AppointmentDate.Hour < 12);
                }
                else if (timeSlot == "afternoon")
                {
                    query = query.Where(a => a.AppointmentDate.Hour >= 12 && a.AppointmentDate.Hour < 17);
                }
                else if (timeSlot == "evening")
                {
                    query = query.Where(a => a.AppointmentDate.Hour >= 17 && a.AppointmentDate.Hour < 22);
                }
            }

            // 2. Sorting Evaluation Hierarchy with Continuous Ascending Timeline Fallback
            if (feeSort == "lowToHigh")
            {
                query = query.OrderBy(a => a.ConsultationFee).ThenBy(a => a.AppointmentDate);
            }
            else if (feeSort == "highToLow")
            {
                query = query.OrderByDescending(a => a.ConsultationFee).ThenBy(a => a.AppointmentDate);
            }
            else // Default Date Sorting Choice
            {
                if (sortBy == "farthest")
                {
                    query = query.OrderByDescending(a => a.AppointmentDate);
                }
                else // Nearest choice: Continuous chronological sequential timeline
                {
                    query = query.OrderBy(a => a.AppointmentDate);
                }
            }

            // Project safely into the target list framework
            return query.Select(a => new PatientAppointmentItem
            {
                AppointmentId = a.AppointmentId,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.FullName,
                Specialization = a.Doctor.Specialization,
                ProfilePhoto = a.Doctor.ProfilePhoto,
                AppointmentDate = a.AppointmentDate,
                ConsultationFee = a.ConsultationFee,
                Status = a.Status
            }).ToList();
        }

        public DetailedAppointmentTicketViewModel GetAppointmentTicketDetails(int appointmentId, int patientId)
        {
            return _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentId == appointmentId && a.PatientId == patientId)
                .Select(a => new DetailedAppointmentTicketViewModel
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    ConsultationFee = a.ConsultationFee,
                    Status = a.Status,
                    PaymentStatus = a.PaymentStatus,
                    CreatedDate = a.CreatedAt,

                    // Raw Mapping - Backed by model fallback checks
                    PatientId = a.PatientId,
                    PatientName = a.Patient.FullName,
                    PatientPhoto = a.Patient.ProfilePhoto,
                    PatientGender = a.Patient.Gender,
                    PatientAge = DateTime.Today.Year - a.Patient.DateOfBirth.Year,

                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.FullName,
                    Specialization = a.Doctor.Specialization,
                    SubSpecialization = a.Doctor.SubSpecialties,
                    DoctorPhoto = a.Doctor.ProfilePhoto
                })
                .FirstOrDefault();
        }
    }
}