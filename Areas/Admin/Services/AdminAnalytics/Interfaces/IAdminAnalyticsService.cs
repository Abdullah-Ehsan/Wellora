using Wellora.Areas.Admin.ViewModels.AdminAnalytics;

namespace Wellora.Areas.Admin.Services.AdminAnalytics.Interfaces;

public interface IAdminAnalyticsService
{
    Task<AdminAnalyticsViewModel> GetAnalyticsAsync();
}
