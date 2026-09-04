namespace Wellora.Areas.Admin.ViewModels.AdminAnalytics;

public class DoctorAnalyticsViewModel
{
    public List<CategoryCountViewModel> Specializations { get; set; } = [];

    public List<CategoryCountViewModel> SubSpecialties { get; set; } = [];

    public List<StatusCountViewModel> GenderDistribution { get; set; } = [];

    public List<ExperienceGroupViewModel> ExperienceGroups { get; set; } = [];

    public List<FeeRangeViewModel> ConsultationFeeDistribution { get; set; } = [];

    public List<CategoryCountViewModel> TelemedicineAvailability { get; set; } = [];

    public List<CategoryCountViewModel> Countries { get; set; } = [];

    public List<DoctorWorkloadViewModel> BusiestDoctors { get; set; } = [];

    public List<DoctorRevenueViewModel> DoctorRevenue { get; set; } = [];

    public List<DoctorPerformanceViewModel> Performance { get; set; } = [];

    public List<CategoryCountViewModel> PrimaryMedicalDegrees { get; set; } = [];

    public List<CategoryCountViewModel> PostgraduateDegrees { get; set; } = [];

    public List<CategoryCountViewModel> SuperSpecialties { get; set; } = [];

    public List<CategoryCountViewModel> ProfessionalCertifications { get; set; } = [];

    public List<CategoryCountViewModel> MedicalSchools { get; set; } = [];

}


public class ExperienceGroupViewModel
{
    public string Range { get; set; } = string.Empty;

    public int Count { get; set; }
}


public class FeeRangeViewModel
{
    public string Range { get; set; } = string.Empty;

    public int Count { get; set; }
}


public class DoctorWorkloadViewModel
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public int AppointmentCount { get; set; }
}


public class DoctorRevenueViewModel
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public decimal Revenue { get; set; }
}


public class DoctorPerformanceViewModel
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public int TotalAppointments { get; set; }

    public int CompletedAppointments { get; set; }

    public int CancelledAppointments { get; set; }

    public decimal Revenue { get; set; }

    public decimal AverageFee { get; set; }
}
