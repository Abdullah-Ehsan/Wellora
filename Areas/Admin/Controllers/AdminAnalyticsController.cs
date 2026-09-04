using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wellora.Areas.Admin.Services.AdminAnalytics.Interfaces;

namespace Wellora.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
public class AdminAnalyticsController : Controller
{
    private readonly IAdminAnalyticsService _adminAnalyticsService;

    public AdminAnalyticsController(
        IAdminAnalyticsService adminAnalyticsService)
    {
        _adminAnalyticsService = adminAnalyticsService;
    }

    // =========================================================
    // ADMIN ANALYTICS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> AdminAnalytics()
    {
        var model = await _adminAnalyticsService.GetAnalyticsAsync();

        return View(model);
    }
}
