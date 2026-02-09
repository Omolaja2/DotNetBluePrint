using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DotNetBlueprint.Services;
using DotNetBlueprint.Models;
using DotNetBlueprint.Data;

namespace DotNetBlueprint.Controllers
{
    [Authorize]
    public class GeneratorController : Controller
    {
        private readonly ProjectGeneratorService _generator;
        private readonly AppDbContext _context;

        public GeneratorController(ProjectGeneratorService generator, AppDbContext context)
        {
            _generator = generator;
            _context = context;
        }

        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult History()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                var history = _context.ProjectBlueprints
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();
                return View(history);
            }
            return RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        public IActionResult Generate(ProjectRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", request);
            }

            byte[] zipBytes;
            try
            {
                zipBytes = _generator.GenerateDotNetProject(
                    request.ProjectName, 
                    request.NetVersion, 
                    request.Architecture.ToString(),
                    request.Database.ToString()
                );
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Project generation failed: {ex.Message}");
                return View("Create", request);
            }

            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
                {
                    var blueprint = new ProjectBlueprint
                    {
                        ProjectName = request.ProjectName,
                        Architecture = request.Architecture.ToString(),
                        DotNetVersion = request.NetVersion,
                        Database = request.Database.ToString(),
                        UserId = userId
                    };
                    _context.ProjectBlueprints.Add(blueprint);
                    _context.SaveChanges();
                }
            }
            catch { /* Log error here if needed */ }

            Response.Cookies.Append("fileDownload", "true", new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = false });

            return File(
                zipBytes,
                "application/zip",
                $"{request.ProjectName}_{request.Architecture}.zip"
            );
        }
    }
}
