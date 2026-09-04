using Wellora.Areas.Doctor.ViewModels.DoctorProfile;

namespace Wellora.Areas.Doctor.Services.DoctorProfile.Interfaces
{
    public interface IDoctorScheduleService
    {
        Task<DoctorScheduleViewModel> GetScheduleAsync(
            int doctorId,
            CancellationToken cancellationToken);

        Task<ScheduleUpdateResult> UpdateScheduleAsync(
            int doctorId,
            int userId,
            DoctorScheduleUpdateViewModel model,
            CancellationToken cancellationToken);

        Task ActivatePendingSchedulesAsync(
            CancellationToken cancellationToken);
    }
}
