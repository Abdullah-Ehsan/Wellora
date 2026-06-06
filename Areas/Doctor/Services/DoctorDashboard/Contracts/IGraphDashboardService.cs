using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;

namespace Wellora.Services.DoctorDashboard.Contracts
{
    public interface IGraphDashboardService
    {
        Task<GraphDataViewModel> GetGraphDataAsync(int doctorId);
    }
}