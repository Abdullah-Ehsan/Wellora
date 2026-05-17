using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Wellora.Data;

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


// Route for areas (must come before default)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Custom 404 handling
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 404)
    {
        response.Redirect("/Error/NotFound");
    }
});




app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");




app.Run();
