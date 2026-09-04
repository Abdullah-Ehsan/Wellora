namespace Wellora.Areas.Admin.ViewModels.DoctorStats;

public class DoctorStatsFinancialViewModel
{
    public List<DoctorStatsRevenueDataPointViewModel> RevenueOverTime { get; set; } = [];

    public List<DoctorStatsMonthlyRevenueViewModel> MonthlyRevenue { get; set; } = [];

    public List<DoctorStatsPaymentMethodViewModel> RevenueByPaymentMethod { get; set; } = [];

    public List<DoctorStatsPaymentStatusViewModel> PaymentStatus { get; set; } = [];

    public string HighestRevenueMonth { get; set; } = string.Empty;

    public decimal HighestMonthlyRevenue { get; set; }
}


public class DoctorStatsRevenueDataPointViewModel
{
    public DateTime Date { get; set; }

    public decimal Amount { get; set; }
}


public class DoctorStatsMonthlyRevenueViewModel
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}


public class DoctorStatsPaymentMethodViewModel
{
    public string PaymentMethod { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int Count { get; set; }
}


public class DoctorStatsPaymentStatusViewModel
{
    public string Status { get; set; } = string.Empty;

    public int Count { get; set; }

    public decimal Amount { get; set; }
}
