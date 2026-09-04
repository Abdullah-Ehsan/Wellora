using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wellora.Areas.Admin.ViewModels.AdminDashboard;
using Wellora.Data;




namespace Wellora.Areas.Admin.Controllers
{
    [Area ("Admin")]
    [Authorize (Roles = "admin")]


    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        


        public async Task<IActionResult> AdminDashboard()
        {
            // 1. Get AdminId from claims
            var adminIdClaim = User.FindFirstValue("CurrentAdminId");

            if (string.IsNullOrEmpty(adminIdClaim))
                return Unauthorized();

            int adminId = int.Parse(adminIdClaim);

            // 2. Load admin + user in one query
            var admin = await _context.Admins
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AdminId == adminId);

            if (admin == null)
                return NotFound();

            // 3. Build ViewModel
            var model = new AdminDashboardViewModel
            {
                AdminId = admin.AdminId,
                UserId = admin.UserId,

                FullName = admin.FullName,
                Username = admin.User?.Username ?? "",
                Email = admin.User?.Email ?? "",

                ProfilePicture = admin.ProfilePicture,
                Gender = admin.Gender,
                ContactNumber = admin.ContactNumber,
                Address = admin.Address,

                DateOfBirth = admin.DateOfBirth,
                AdminType = admin.AdminType,
                Status = admin.Status
            };

            return View(model);
        }
    }
}
