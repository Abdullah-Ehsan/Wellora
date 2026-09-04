using System.Collections.Generic;
using Wellora.Areas.Patient.Models;
using Wellora.Models;

namespace Wellora.ViewModels.PatientDetail
{
    public class PatientDetailViewModel
    {
        public Patient Patient { get; set; }

        public Appointment CurrentAppointment { get; set; }

        public List<AppointmentSummaryViewModel> PreviousAppointments { get; set; } = new();

        public string Age { get; set; }

        public int TotalAppointments { get; set; }

        public AppointmentSummaryViewModel LastAppointment { get; set; }

        public string LastAppointmentTimePassed { get; set; }

        // Primary doctor information
        public PrimaryDoctorViewModel PrimaryDoctor { get; set; }

        // Appointment actions/status
        public AppointmentActionsViewModel AppointmentActions { get; set; }
    }
}
