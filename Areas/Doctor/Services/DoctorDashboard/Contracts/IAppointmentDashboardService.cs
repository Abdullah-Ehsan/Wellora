using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;

namespace Wellora.Services.DoctorDashboard.Contracts
{
    public interface IAppointmentDashboardService
    {
        Task<List<TodayAppointmentViewModel>> GetTodayAppointmentsAsync(int doctorId);

        Task<List<TodayAppointmentViewModel>> GetRecentAppointmentsAsync(int doctorId, int days = 7);
    }
}