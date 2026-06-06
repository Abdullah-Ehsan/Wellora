using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;

namespace Wellora.Services.DoctorDashboard.Contracts
{
    public interface IScheduleDashboardService
    {
        Task<List<WeeklyScheduleRowViewModel>> GetWeeklyScheduleAsync(int doctorId);
    }
}