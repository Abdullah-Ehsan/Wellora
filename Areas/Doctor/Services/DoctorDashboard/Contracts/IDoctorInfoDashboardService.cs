using Wellora.Areas.Doctor.ViewModels.DoctorDashboard;

namespace Wellora.Areas.Doctor.Services.DoctorDashboard.Contracts
{
    public interface IDoctorInfoDashboardService
    {
        Task<DoctorHeaderViewModel> GetDoctorHeaderAsync(int doctorId);

        Task<DoctorPersonalInfoViewModel> GetDoctorPersonalInfoAsync(int doctorId);

        Task<ClinicalPracticeViewModel> GetClinicalPracticeAsync(int doctorId);

        Task<CredentialsViewModel> GetCredentialsAsync(int doctorId);

        Task<SpecialtiesViewModel> GetSpecialtiesAsync(int doctorId);

        Task<PublicationViewModel> GetPublicationsAsync(int doctorId);
    }
}
