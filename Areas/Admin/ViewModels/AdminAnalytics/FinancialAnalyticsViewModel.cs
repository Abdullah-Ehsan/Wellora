namespace Wellora.Areas.Admin.ViewModels.AdminAnalytics;

public class FinancialAnalyticsViewModel
{
    public List<RevenueDataPointViewModel> RevenueOverTime { get; set; } = [];

    public List<StatusCountViewModel> PaymentStatus { get; set; } = [];

    public List<PaymentMethodCountViewModel> PaymentMethods { get; set; } = [];

    public List<PaymentMethodRevenueViewModel> RevenueByPaymentMethod { get; set; } = [];

    public List<StatusCountViewModel> TransactionOutcomes { get; set; } = [];
}


public class RevenueDataPointViewModel
{
    public DateTime Date { get; set; }

    public decimal Amount { get; set; }
}


public class StatusCountViewModel
{
    public string Status { get; set; } = string.Empty;

    public int Count { get; set; }
}


public class PaymentMethodCountViewModel
{
    public string PaymentMethod { get; set; } = string.Empty;

    public int Count { get; set; }
}


public class PaymentMethodRevenueViewModel
{
    public string PaymentMethod { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
