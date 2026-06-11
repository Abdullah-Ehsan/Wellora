using System;

namespace Wellora.ViewModels.PatientDetail
{
    public class AppointmentSummaryViewModel
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
    }
}
