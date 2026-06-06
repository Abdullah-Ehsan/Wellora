namespace Wellora.Areas.Doctor.ViewModels.DoctorDashboard
{
    public class RevenueViewModel
    {
        public decimal TodayRevenue { get; set; }

        public decimal MonthlyRevenue { get; set; }

        public decimal TotalRevenue { get; set; }

        public int CompletedAppointments { get; set; }
    }
}