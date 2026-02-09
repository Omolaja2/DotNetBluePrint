using DotNetBlueprint.Data;
using DotNetBlueprint.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

// Disable file watching to prevent 'inotify' limit errors on Render
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllersWithViews();

// Retreive connection string from both appsettings and Environment Variables
// We check multiple locations for maximum compatibility with Render/Production
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
                      ?? builder.Configuration["DefaultConnection"];

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("WARNING: Connection string 'DefaultConnection' not found in configuration!");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString ?? "Server=placeholder;Database=placeholder", 
        new MySqlServerVersion(new Version(8, 0, 35)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null))
);


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddScoped<ProjectGeneratorService>();
builder.Services.AddScoped<ZipService>();
builder.Services.AddScoped<IEmailService, EmailService>();



var app = builder.Build();

// Automatically ensure DB is created on startup (Speed Optimized)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("Starting database connectivity check...");
        
        // Use a short timeout so we don't hang the app and cause a 'Bad Gateway'
        db.Database.SetCommandTimeout(10); 
        
        if (db.Database.CanConnect())
        {
            db.Database.EnsureCreated();
            logger.LogInformation("✅ Database forged and ready!");
        }
        else
        {
            logger.LogWarning("⚠️ Database is not reachable yet. The app will start, but database features will be offline.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError("❌ Database initialization skipped: {Message}", ex.Message);
    }
}



if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
