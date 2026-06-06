using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;

namespace Wellora.Services.DoctorDashboard.Contracts
{
    public interface IPatientDashboardService
    {
        Task<PatientStatsViewModel> GetPatientStatsAsync(int doctorId);

        Task<List<PatientSummaryViewModel>> GetTopSpendingPatientsAsync(int doctorId, int top = 1);

        Task<List<PatientSummaryViewModel>> GetMostVisitedPatientsAsync(int doctorId, int top = 1);
    }
}