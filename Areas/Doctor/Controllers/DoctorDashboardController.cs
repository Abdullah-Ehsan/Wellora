using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wellora.Areas.Doctor.Services.DoctorDashboard.DoctorDashboardService;
using Wellora.Services.DoctorDashboard.Contracts;

namespace Wellora.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "doctor")]
    public class DoctorDashboardController : Controller
    {
        private readonly IDoctorDashboardService _dashboardService;

        public DoctorDashboardController(IDoctorDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // =========================================
        // DOCTOR DASHBOARD MAIN PAGE
        // =========================================
        public async Task<IActionResult> DoctorDashboard()
        {
            var doctorIdClaim = User.FindFirst("CurrentDoctorId")?.Value;

            if (!int.TryParse(doctorIdClaim, out int doctorId))
            {
                return Unauthorized();
            }

            var model = await _dashboardService.GetDashboardAsync(doctorId);

            

            return View(model);
        }
    }
}