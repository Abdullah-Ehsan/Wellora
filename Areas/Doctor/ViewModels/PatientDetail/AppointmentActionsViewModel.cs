namespace Wellora.ViewModels.PatientDetail
{
    public class AppointmentActionsViewModel
    {
        public string? ScheduledStatus { get; set; }

        public string? PaymentStatus { get; set; }

        public bool CanConfirm { get; set; }

        public bool CanCancel { get; set; }

        public bool CanMarkPaid { get; set; }
    }
}
