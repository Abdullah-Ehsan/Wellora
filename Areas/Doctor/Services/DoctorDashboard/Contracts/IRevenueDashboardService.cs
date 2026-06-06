using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;

namespace Wellora.Services.DoctorDashboard.Contracts
{
    public interface IRevenueDashboardService
    {
        Task<RevenueViewModel> GetRevenueAsync(int doctorId);
    }
}