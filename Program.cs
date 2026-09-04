using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using QuestPDF.Infrastructure;
using Stripe;
using Wellora.Areas.Admin.Services.AdminAnalytics.Interfaces;
using Wellora.Areas.Admin.Services.AdminAnalytics.Services;
using Wellora.Areas.Admin.Services.DoctorStats.Interfaces;
using Wellora.Areas.Admin.Services.DoctorStats.Services;
using Wellora.Areas.Doctor.Services;
using Wellora.Areas.Doctor.Services.DoctorDashboard.Contracts;
using Wellora.Areas.Doctor.Services.DoctorDashboard.DoctorDashboardService;
using Wellora.Areas.Doctor.Services.DoctorDashboard.Services;
using Wellora.Areas.Doctor.Services.DoctorProfile;
using Wellora.Areas.Doctor.Services.DoctorProfile.Interfaces;
using Wellora.Areas.Doctor.Services.DoctorProfile.Services;
using Wellora.Areas.Patient.Services.PatientProfile;
using Wellora.Areas.Patient.Services.Scheduling;
using Wellora.Data;
using Wellora.Services;
using Wellora.Services.Dashboard;
using Wellora.Services.DoctorDashboard;
using Wellora.Services.DoctorDashboard.Contracts;
using Wellora.Services.DoctorDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

StripeConfiguration.ApiKey =
    builder.Configuration["Stripe:SecretKey"];

// Add MySQL connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                var path = context.Request.Path;

                if (path.StartsWithSegments("/Doctor"))
                {
                    context.Response.Redirect("/Doctor/DoctorAccount/DoctorLogin");
                }
                else if (path.StartsWithSegments("/Admin"))
                {
                    context.Response.Redirect("/Admin/AdminAccount/AdminLogin");
                }
                else if (path.StartsWithSegments("/Patient"))
                {
                    context.Response.Redirect("/Patient/PatientAccount/PatientLogin");
                }
                else
                {
                    context.Response.Redirect("/Home/Index");
                }

                return Task.CompletedTask;
            },

            OnRedirectToAccessDenied = context =>
            {
                var path = context.Request.Path;

                if (path.StartsWithSegments("/Doctor"))
                {
                    context.Response.Redirect("/Doctor/DoctorAccount/AccessDenied");
                }
                else if (path.StartsWithSegments("/Admin"))
                {
                    context.Response.Redirect("/Admin/AdminAccount/AccessDenied");
                }
                else if (path.StartsWithSegments("/Patient"))
                {
                    context.Response.Redirect("/Patient/PatientAccount/AccessDenied");
                }
                else
                {
                    context.Response.Redirect("/Home/Index");
                }

                return Task.CompletedTask;
            }
        };
    });


//------------------------Home Section


// Conact page
builder.Services.AddScoped<IEmailService, EmailService>();










//========================Areas====================================


//---------------------Patient Services-----------------------------
builder.Services.AddScoped<AppointmentSlotService>();
builder.Services.AddScoped<PatientAppointmentService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<PatientProfileService>();





//---------------------------Doctor Services----------------------------

//Dashboard of the doctor
builder.Services.AddScoped<IDoctorDashboardService, DoctorDashboardService>();
builder.Services.AddScoped<IDoctorInfoDashboardService, DoctorInfoDashboardService>();
builder.Services.AddScoped<IAppointmentDashboardService, AppointmentDashboardService>();
builder.Services.AddScoped<IPatientDashboardService, PatientDashboardService>();
builder.Services.AddScoped<IRevenueDashboardService, RevenueDashboardService>();
builder.Services.AddScoped<IGraphDashboardService, GraphDashboardService>();
builder.Services.AddScoped<IScheduleDashboardService, ScheduleDashboardService>();


//Doctor Profile
builder.Services.AddScoped<IDoctorProfileService, DoctorProfileService>();
builder.Services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();






//-------------------------------Admin-----------------------------------------

//Admin Analitics
builder.Services.AddScoped<IAdminAnalyticsService, AdminAnalyticsService>();

//Doctor Stats
builder.Services.AddScoped<IDoctorStatsService, DoctorStatsService>();







var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

// Custom 404 handling
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 404)
    {
        response.Redirect("/Error/NotFound");
    }
});

// Route for areas (must come before default)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");




app.Run();
