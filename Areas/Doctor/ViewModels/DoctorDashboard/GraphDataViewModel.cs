namespace Wellora.Areas.Doctor.ViewModels.DoctorDashboard
{
    public class GraphDataViewModel
    {
        public List<string> MonthlyVisitLabels { get; set; } = new();
        public List<int> MonthlyVisitValues { get; set; } = new();

        public List<string> WeeklyVisitLabels { get; set; } = new();
        public List<int> WeeklyVisitValues { get; set; } = new();

        public List<string> RevenueLabels { get; set; } = new();
        public List<decimal> RevenueValues { get; set; } = new();
    }
}