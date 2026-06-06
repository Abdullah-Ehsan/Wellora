using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;

namespace Wellora.Areas.Doctor.Services.DoctorDashboard.DoctorDashboardService
{
    public interface IDoctorDashboardService
    {
        Task<DoctorDashboardViewModel> GetDashboardAsync(int doctorId);
    }
}
