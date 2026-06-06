using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Wellora.Areas.Doctor.Services.DoctorDashboard.DoctorDashboardService;
using Wellora.Areas.Patient.Services.PatientProfile;
using Wellora.Areas.Patient.Services.Scheduling;
using Wellora.Data;
using Wellora.Services;
using Wellora.Services.Dashboard;
using Wellora.Services.DoctorDashboard;
using Wellora.Services.DoctorDashboard.Contracts;
using Wellora.Services.DoctorDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MySQL connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

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
                else
                {
                    context.Response.Redirect("/Patient/PatientAccount/PatientLogin");
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
                else
                {
                    context.Response.Redirect("/Patient/PatientAccount/AccessDenied");
                }

                return Task.CompletedTask;
            }
        };
    });


//Home Section Conact page
builder.Services.AddScoped<IEmailService, EmailService>();



//Areas

//Patient Services
builder.Services.AddScoped<AppointmentSlotService>();
builder.Services.AddScoped<PatientAppointmentService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<PatientProfileService>();





//Doctor Services

//Dashboard of the doctor
builder.Services.AddScoped<IDoctorDashboardService, DoctorDashboardService>();

builder.Services.AddScoped<IAppointmentDashboardService, AppointmentDashboardService>();
builder.Services.AddScoped<IPatientDashboardService, PatientDashboardService>();
builder.Services.AddScoped<IRevenueDashboardService, RevenueDashboardService>();
builder.Services.AddScoped<IGraphDashboardService, GraphDashboardService>();
builder.Services.AddScoped<IScheduleDashboardService, ScheduleDashboardService>();


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
