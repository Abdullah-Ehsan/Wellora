namespace Wellora.Areas.Admin.ViewModels.AdminAnalytics;

public class PatientAnalyticsViewModel
{
    public List<PatientVisitViewModel> TopVisitedPatients { get; set; } = [];

    public List<PatientSpendingViewModel> TopSpendingPatients { get; set; } = [];

    public List<StatusCountViewModel> GenderDistribution { get; set; } = [];

    public List<AgeGroupViewModel> AgeGroups { get; set; } = [];

    public List<CategoryCountViewModel> PreferredLanguages { get; set; } = [];

    public List<DoctorPatientCountViewModel> PatientsByPrimaryDoctor { get; set; } = [];
}


public class PatientVisitViewModel
{
    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public int VisitCount { get; set; }
}


public class PatientSpendingViewModel
{
    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public decimal TotalSpent { get; set; }
}


public class AgeGroupViewModel
{
    public string AgeGroup { get; set; } = string.Empty;

    public int Count { get; set; }
}


public class CategoryCountViewModel
{
    public string Category { get; set; } = string.Empty;

    public int Count { get; set; }
}


public class DoctorPatientCountViewModel
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public int PatientCount { get; set; }
}
