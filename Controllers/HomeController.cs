using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using Wellora.Areas.Patient.Models;
using Wellora.Data;
using Wellora.Models;
using Wellora.Services;
using Wellora.ViewModels;

namespace Wellora.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IEmailService emailService)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            var users = _context.Users.ToList(); // read all users
            return View(users); // pass to view
            //return View();
        }
       

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            var vm = new ContactViewModel();
            return View(vm);
        }

        [HttpPost]
        public IActionResult SendMessage(ContactViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var fullName = $"{vm.Form.FirstName} {vm.Form.LastName}";
                _emailService.SendEmail(
                    $"New message from {fullName}",
                    $"Email: {vm.Form.Email}\nMessage: {vm.Form.Message}"
                );

                ViewBag.Message = "Your message has been sent successfully!";
            }
            return View("Contact", vm);
        }

        [HttpPost]
        public IActionResult Subscribe(string NewsletterEmail)
        {
            // Save subscription or send confirmation
            ViewBag.Message = "Subscribed successfully!";
            return RedirectToAction("Contact");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        

    }
}
