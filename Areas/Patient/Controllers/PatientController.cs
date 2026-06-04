using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wellora.Data; 
using Wellora.Services;
using Wellora.Services.Dashboard;
using Wellora.Areas.Patient.ViewModels;
using Wellora.Areas.Patient.Services.Scheduling;

namespace Wellora.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PatientAppointmentService _appointmentService;
        private readonly DashboardService _dashboardService;

        public PatientController(
            ApplicationDbContext context,
            PatientAppointmentService appointmentService,
            DashboardService dashboardService)
        {
            _context = context;
            _appointmentService = appointmentService;
            _dashboardService = dashboardService;
        }

        public IActionResult PatientProfile()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PatientDashboard()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var modelData = _dashboardService.GetPatientDashboardData(userId);
            if (modelData == null) return NotFound("Patient database context not established.");

            return View(modelData);
        }

        public IActionResult AIChat()
        {
            return View();
        }

        
    }
}
