using Microsoft.AspNetCore.Mvc;

namespace Wellora.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult NotFound()
        {
            string area = (RouteData.Values["area"] ?? "").ToString();
            string message;

            // Role-based message
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Doctor"))
                    message = "Sorry Doctor, the page you’re looking for doesn’t exist.";
                else if (User.IsInRole("Patient"))
                    message = "Sorry Patient, we couldn’t find that page.";
                else if (User.IsInRole("Admin"))
                    message = "Sorry Admin, this page is missing or unavailable.";
                else
                    message = "Sorry User, page not found.";
            }
            else
            {
                message = "Sorry Visitor, please log in to continue.";
            }

            // Area-specific layout selection
            string layoutPath;
            switch (area)
            {
                case "Doctor":
                    layoutPath = "~/Areas/Doctor/Views/Shared/_Layout.cshtml";
                    break;
                case "Patient":
                    layoutPath = "~/Areas/Patient/Views/Shared/_Layout.cshtml";
                    break;
                case "Admin":
                    layoutPath = "~/Areas/Admin/Views/Shared/_Layout.cshtml";
                    break;
                default:
                    layoutPath = "~/Views/Shared/_Layout.cshtml"; // fallback for visitors
                    break;
            }

            ViewBag.Message = message;
            ViewBag.Layout = layoutPath;
            ViewBag.Area = area;

            return View();
        }
    }
}
