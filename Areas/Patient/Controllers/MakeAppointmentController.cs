using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Wellora.Areas.Patient.Services.Scheduling;
using Wellora.Areas.Patient.ViewModels.MakeAppointment;
using Wellora.Data;
using Wellora.Models;

namespace Wellora.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = "patient")]


    public class MakeAppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AppointmentSlotService _slotService;
        private readonly PatientAppointmentService _appointmentService;
        private readonly IConfiguration _configuration;
        private readonly ICompositeViewEngine _viewEngine;


        public MakeAppointmentController(
            ApplicationDbContext context,
            AppointmentSlotService slotService,
            PatientAppointmentService appointmentService,
            IConfiguration configuration,
            ICompositeViewEngine viewEngine)
        {
            _context = context;
            _slotService = slotService;
            _appointmentService = appointmentService;
            _configuration = configuration;
            _viewEngine = viewEngine;
        }



        //================
        //payment conmfirm from the stripe
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body)
                .ReadToEndAsync();

            var stripeSignature = Request.Headers["Stripe-Signature"];

            try
            {
                var webhookSecret = _configuration["Stripe:WebhookSecret"];

                var stripeEvent = Stripe.EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    webhookSecret
                );

                if (stripeEvent.Type == Stripe.EventTypes.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                    if (session != null &&
                        session.Metadata.TryGetValue(
                            "AppointmentId",
                            out var appointmentIdString) &&
                        int.TryParse(appointmentIdString, out var appointmentId))
                    {
                        // ==========================================
                        // FIND APPOINTMENT
                        // ==========================================

                        var appointment = await _context.Appointments
                            .FirstOrDefaultAsync(a =>
                                a.AppointmentId == appointmentId);

                        if (appointment != null)
                        {
                            // ==========================================
                            // UPDATE APPOINTMENT
                            // ==========================================

                            appointment.PaymentStatus = "paid";
                            appointment.PaymentMethod = "Online";

                            // ==========================================
                            // FIND TRANSACTION
                            // ==========================================

                            var transaction = await _context.Transactions
                                .FirstOrDefaultAsync(t =>
                                    t.AppointmentId == appointmentId);

                            if (transaction != null)
                            {
                                // ==========================================
                                // UPDATE TRANSACTION
                                // ==========================================

                                transaction.Status = "paid";

                                // Stripe Checkout Session ID
                                transaction.StripeSessionId = session.Id;

                                // Stripe Payment Intent ID
                                transaction.StripePaymentIntentId =
                                    session.PaymentIntentId;

                                transaction.Timestamp = DateTime.Now;
                            }

                            // ==========================================
                            // SAVE BOTH
                            // ==========================================

                            await _context.SaveChangesAsync();
                        }
                    }
                }


                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
        }

        private string RenderPartialViewToString(
            string viewName,
            object model)
        {
            ViewData.Model = model;

            using var writer = new StringWriter();

            var viewResult = _viewEngine.FindView(
                ControllerContext,
                viewName,
                false
            );

            if (!viewResult.Success)
            {
                throw new InvalidOperationException(
                    $"View '{viewName}' was not found."
                );
            }

            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                ViewData,
                TempData,
                writer,
                new HtmlHelperOptions()
            );

            viewResult.View.RenderAsync(viewContext)
                .GetAwaiter()
                .GetResult();

            return writer.GetStringBuilder().ToString();


        }

        // =========================
        // BOOKING PAGE
        // =========================
        public IActionResult AppointmentBooking(int doctorId, int? year, int? month)
        {
            var doctor = _context.Doctors
                .FirstOrDefault(d => d.DoctorId == doctorId);

            if (doctor == null) return NotFound();

            var targetYear = year ?? DateTime.Today.Year;
            var targetMonth = month ?? DateTime.Today.Month;

            ViewBag.Year = targetYear;
            ViewBag.Month = targetMonth;

            var availableDates = GenerateAvailableDates(doctorId);

            var vm = new AppointmentBookingViewModel
            {
                DoctorId = doctor.DoctorId,
                DoctorName = doctor.FullName,
                Specialization = doctor.Specialization,
                SubSpecialization = doctor.SubSpecialties,
                ProfilePhoto = doctor.ProfilePhoto,
                ConsultationFee = doctor.ConsultationFee,

                AvailableDates = availableDates,
                CalendarCells = BuildCalendarCells(availableDates, targetYear, targetMonth)
            };

            return View("AppointmentBooking", vm);
        }

        // =========================
        // GET SLOTS (AJAX or reload)
        // =========================
        [HttpGet("/Patient/MakeAppointment/GetAvailableSlots")]
        public IActionResult GetSlots(int doctorId, DateTime date)
        {
            var result = _slotService.GenerateSlots(doctorId, date);

            return Json(new
            {
                morningSlots = result.Morning,
                afternoonSlots = result.Afternoon,
                eveningSlots = result.Evening,
                noSlots = result.NoSlots
            });
        }

        // =========================
        // CONFIRM BOOKING
        // =========================
        [HttpPost]
        public IActionResult Confirm(AppointmentBookingViewModel vm)
        {
            if (vm.SelectedDate == null || string.IsNullOrEmpty(vm.SelectedSlot))
            {
                ModelState.AddModelError("", "Please select date and slot.");
                return RedirectToAction("AppointmentBooking", new
                {
                    doctorId = vm.DoctorId
                });
            }

            // rebuild full datetime
            var slotDateTime = DateTime.Parse($"{vm.SelectedDate:yyyy-MM-dd} {vm.SelectedSlot}");

            // DOUBLE BOOKING PROTECTION
            var exists = _context.Appointments.Any(a =>
                a.DoctorId == vm.DoctorId &&
                a.AppointmentDate == slotDateTime &&
                a.Status != "cancelled");

            if (exists)
            {
                TempData["Error"] = "This slot is already booked.";
                return RedirectToAction("AppointmentBooking", new
                {
                    doctorId = vm.DoctorId
                });
            }

            // get patient
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId);

            var patientId = _context.Patients
                .Where(p => p.UserId == userId)
                .Select(p => p.PatientId)
                .FirstOrDefault();

            if (patientId == 0)
                return Unauthorized();

            var appointment = new Appointment
            {
                DoctorId = vm.DoctorId,
                PatientId = patientId,
                AppointmentDate = slotDateTime,
                Status = "scheduled",
                PaymentStatus = "pending",
                PaymentMethod = vm.PaymentMethod,
                ConsultationFee = vm.ConsultationFee,
                Notes = vm.Notes,
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();


            var transaction = new Transaction
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = patientId,
                Amount = appointment.ConsultationFee,
                PaymentMethod = vm.PaymentMethod == "Onsite"
                    ? "cash"
                    : "online",
                Status = "pending",
                Timestamp = DateTime.Now
            };
            _context.Transactions.Add(transaction);
            _context.SaveChanges();
            //return RedirectToAction("PatientAppointment");

            // ==========================================
            // ONSITE PAYMENT
            // ==========================================

            if (vm.PaymentMethod == "Onsite")
            {
                return RedirectToAction("PatientAppointment");
            }

            // ==========================================
            // ONLINE PAYMENT
            // ==========================================

            if (vm.PaymentMethod == "Online")
            {
                var domain = $"{Request.Scheme}://{Request.Host}";

                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    Mode = "payment",

                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                    {
                        new Stripe.Checkout.SessionLineItemOptions
                        {
                            PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                            {
                                Currency = "pkr",

                                UnitAmount =
                                (long)(vm.ConsultationFee * 100),

                                ProductData =
                                new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Doctor Consultation"
                                }
                             },

                            Quantity = 1
                        }
                    },

                    Metadata = new Dictionary<string, string>
                    {
                        {
                            "AppointmentId",
                                appointment.AppointmentId.ToString()
                        }
                    },

                    SuccessUrl =
                        $"{domain}/Patient/MakeAppointment/PaymentSuccess?session_id={{CHECKOUT_SESSION_ID}}",

                    CancelUrl =
                        $"{domain}/Patient/MakeAppointment/PaymentCancel"
                };

                var service = new Stripe.Checkout.SessionService();

                var session = service.Create(options);

                return Redirect(session.Url);
            }

            return RedirectToAction("PatientAppointment");

        }

        // =========================
        // AVAILABLE DATES (6 MONTH RULE)
        // =========================
        private List<DateTime> GenerateAvailableDates(int doctorId)
        {
            var today = DateTime.Today;
            var end = today.AddMonths(6);

            var schedules = _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && s.IsActive)
                .ToList();

            var result = new List<DateTime>();

            for (var d = today; d <= end; d = d.AddDays(1))
            {
                int dbDay = (int)d.DayOfWeek == 0 ? 7 : (int)d.DayOfWeek;

                if (schedules.Any(s => s.DayOfWeek == dbDay))
                    result.Add(d);
            }

            return result;
        }

        // =========================
        // CALENDAR BUILDER
        // =========================
        private List<CalendarCell> BuildCalendarCells(
            List<DateTime> availableDates,
            int year,
            int month)
        {
            var cells = new List<CalendarCell>();

            var start = new DateTime(year, month, 1);
            var days = DateTime.DaysInMonth(year, month);

            var offset = ((int)start.DayOfWeek + 6) % 7; // Monday=0
            int day = 1;

            for (int i = 0; i < 42; i++) // 6x7 grid
            {
                if (i < offset || day > days)
                {
                    cells.Add(new CalendarCell { Date = null });
                }
                else
                {
                    var date = new DateTime(year, month, day);

                    cells.Add(new CalendarCell
                    {
                        Date = date,
                        IsAvailable = availableDates.Contains(date)
                    });

                    day++;
                }
            }

            return cells;
        }

        [HttpGet]
        public IActionResult GetCalendar(int doctorId, int year, int month)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == doctorId);
            if (doctor == null) return NotFound();

            var availableDates = GenerateAvailableDates(doctor.DoctorId);
            var cells = BuildCalendarCells(availableDates, year, month);

            return PartialView("_CalendarPartial", cells);
        }








        //this the second page of this controller

        [HttpGet]
        public IActionResult PatientAppointment()
        {
            return View();
        }

        // Secure AJAX JSON Endpoint calling the external Service Layer
        // Secure AJAX JSON/HTML Endpoint calling the external Service Layer
        [HttpGet("/Patient/MakeAppointment/GetUpcomingAppointments")]
        public IActionResult GetUpcomingAppointments(
            string sortBy = "nearest",
            string feeSort = "",
            string timeSlot = "",
            int page = 1)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var patientId = _context.Patients
                .Where(p => p.UserId == userId)
                .Select(p => p.PatientId)
                .FirstOrDefault();

            if (patientId == 0)
            {
                return PartialView("_AppointmentTableRows",
                    new List<PatientAppointmentItem>());
            }

            // Safety
            if (page < 1)
                page = 1;

            const int pageSize = 14;

            // Get all filtered appointments from your service
            var allAppointments =
                _appointmentService.GetFilteredAppointmentsForPatient(
                    patientId,
                    sortBy,
                    feeSort,
                    timeSlot
                ).ToList();

            // Total number of appointments
            var totalAppointments = allAppointments.Count;

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(
                totalAppointments / (double)pageSize
            );

            // If requested page is outside range
            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            // Get only the 14 appointments for this page
            var appointments = allAppointments
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass pagination information to partial
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalAppointments = totalAppointments;

            return PartialView(
                "_AppointmentTableRows",
                appointments
            );
        }



        //Page 3 this is only view the ticket for the appointment

        // Add this method inside your existing MakeAppointmentController class
        [HttpGet]
        public IActionResult ViewTicket(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = int.Parse(userIdClaim);

            // Get patient context ID
            var patientId = _context.Patients
                .Where(p => p.UserId == userId)
                .Select(p => p.PatientId)
                .FirstOrDefault();

            if (patientId == 0) return RedirectToAction("PatientAppointments");

            // Fetch unified record details using our secure method
            var ticketDetails = _appointmentService.GetAppointmentTicketDetails(id, patientId);
            if (ticketDetails == null) return NotFound();

            return View(ticketDetails);
        }


        //payments views

        [HttpGet]
        public IActionResult PaymentSuccess(string session_id)
        {
            return View();
        }

        [HttpGet]
        public IActionResult PaymentCancel()
        {
            return View();
        }

    }
}