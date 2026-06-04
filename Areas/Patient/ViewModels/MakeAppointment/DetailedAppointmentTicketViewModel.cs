using System;

namespace Wellora.Areas.Patient.ViewModels.MakeAppointment
{
    public class DetailedAppointmentTicketViewModel
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public decimal ConsultationFee { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        // Patient Table Profile Context
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? PatientPhoto { get; set; }
        public string? PatientGender { get; set; }
        public int PatientAge { get; set; }

        // Doctor Table Profile Context
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string SubSpecialization { get; set; } = string.Empty;
        public string? DoctorPhoto { get; set; }
    }
}