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

// Retreive connection string from all possible sources
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
                      ?? builder.Configuration["DefaultConnection"]
                      ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                      ?? Environment.GetEnvironmentVariable("DefaultConnection");

Console.WriteLine($"[STARTUP] Searching for Connection String...");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("[STARTUP] ❌ WARNING: No Connection String found! (DefaultConnection)");
}
else 
{
    Console.WriteLine($"[STARTUP]  Connection String detected (Length: {connectionString.Length})");
}


string FinalizeConnectionString(string input)
{
    if (string.IsNullOrEmpty(input)) return input;
    if (input.StartsWith("mysql://"))
    {
        try 
        {
            var uri = new Uri(input);
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : "";
            var host = uri.Host;
            var port = uri.Port;
            var database = uri.AbsolutePath.TrimStart('/');
            return $"Server={host};Port={port};Database={database};User Id={user};Password={password};SSL Mode=Required;TrustServerCertificate=true;Connection Timeout=60;";
        }
        catch { return input; }

    }
    return input;
}

var dbConnectionString = FinalizeConnectionString(connectionString!);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(dbConnectionString ?? "Server=placeholder;Database=placeholder", 
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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("Starting database connectivity check...");
        
        db.Database.SetCommandTimeout(10); 
        
        if (db.Database.CanConnect())
        {
            db.Database.EnsureCreated();
            logger.LogInformation(" Database forged and ready!");
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



if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
