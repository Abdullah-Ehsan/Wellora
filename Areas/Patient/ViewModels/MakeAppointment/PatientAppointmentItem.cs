using System;

namespace Wellora.Areas.Patient.ViewModels.MakeAppointment
{
    public class PatientAppointmentItem
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set; }
        public DateTime AppointmentDate { get; set; }
        public decimal ConsultationFee { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}