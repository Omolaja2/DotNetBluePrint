using Microsoft.AspNetCore.Mvc;
using DotNetBlueprint.Services;
using DotNetBlueprint.Models;

namespace DotNetBlueprint.Controllers
{
    public class GeneratorController : Controller
    {
        private readonly ProjectGeneratorService _generator;

        public GeneratorController(ProjectGeneratorService generator)
        {
            _generator = generator;
        }

        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Generate(ProjectRequest request)
        {
            // Validation
            if (!ModelState.IsValid)
            {
                return View("Create", request);
            }

            // Generate project
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

            // Return ZIP file
            return File(
                zipBytes,
                "application/zip",
                $"{request.ProjectName}_{request.Architecture}.zip"
            );
        }
    }
}
