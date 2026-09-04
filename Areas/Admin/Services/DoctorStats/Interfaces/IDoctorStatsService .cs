using Wellora.Areas.Admin.ViewModels.DoctorStats;

namespace Wellora.Areas.Admin.Services.DoctorStats.Interfaces;

public interface IDoctorStatsService
{
    Task<DoctorStatsViewModel?> GetDoctorStatsAsync(int doctorId);
}
