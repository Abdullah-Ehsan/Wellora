namespace Wellora.Areas.Doctor.ViewModels.PatientAppointment
{
    public class PatientAppointmentListViewModel
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
        public string PatientName { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string ProfilePhoto { get; set; }
        public DateTime? LastVisitedDate { get; set; }
        public bool IsFirstAppointment => LastVisitedDate == null;
    }

}
