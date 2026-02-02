using System.Diagnostics;

namespace DotNetBlueprint.Services
{
    public class ProjectGeneratorService
    {
        private readonly ZipService _zipService;

        public ProjectGeneratorService(ZipService zipService)
        {
            _zipService = zipService;
        }

        public byte[] GenerateDotNetProject(string projectName, string netVersion, string architecture, string database)
        {
            projectName = projectName.Trim();
            projectName = new string(projectName.Where(char.IsLetterOrDigit).ToArray());

            if (string.IsNullOrWhiteSpace(projectName))
                throw new Exception("Invalid project name");

            var efVersion = GetEFCoreVersion(netVersion);

            var tempRoot = Path.Combine(Path.GetTempPath(), "DotNetBlueprint");
            Directory.CreateDirectory(tempRoot);

            var projectRoot = Path.Combine(tempRoot, $"{projectName}_{Guid.NewGuid():N}");
            Directory.CreateDirectory(projectRoot);

            var framework = $"net{netVersion}";
            RunCmd("dotnet --version", projectRoot);
            RunCmd($"dotnet new sln -n {projectName}", projectRoot);

            // Generate architecture
            switch (architecture)
            {
                case "Layered":
                    GenerateLayered(projectRoot, projectName, efVersion, database, framework);
                    break;
                case "MVC":
                    GenerateMVC(projectRoot, projectName, framework);
                    break;
                case "CleanArchitecture":
                    GenerateCleanArchitecture(projectRoot, projectName, efVersion, database, framework);
                    break;
                case "MVVM":
                    GenerateMVVM(projectRoot, projectName, framework);
                    break;
                case "Microservices":
                    GenerateMicroservices(projectRoot, projectName, efVersion, database, framework);
                    break;
                case "Hexagonal":
                    GenerateHexagonal(projectRoot, projectName, efVersion, database, framework);
                    break;
                case "CQRS":
                    GenerateCQRS(projectRoot, projectName, efVersion, database, framework);
                    break;
                default:
                    throw new Exception("Unknown architecture type: " + architecture);
            }

            var zipBytes = _zipService.CreateZipAsBytes(projectRoot);
            Directory.Delete(projectRoot, true);

            return zipBytes;
        }

        #region Architecture Generators

        private void GenerateLayered(string projectRoot, string projectName, string efVersion, string database, string framework)
        {
            RunCmd($"dotnet new classlib -n Core --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new classlib -n Business --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new classlib -n Data --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new mvc -n Web --framework {framework} --no-restore", projectRoot);

            RunCmd("dotnet sln add Core Business Data Web", projectRoot);

            RunCmd("dotnet add Business reference Core", projectRoot);
            RunCmd("dotnet add Data reference Business Core", projectRoot);
            RunCmd("dotnet add Web reference Business Data", projectRoot);

            RunCmd($"dotnet add Data package {GetDbPackage(database)} --version {efVersion} --no-restore", projectRoot);

            Directory.CreateDirectory(Path.Combine(projectRoot, "Core", "Entities"));
            File.WriteAllText(Path.Combine(projectRoot, "Core", "Entities", "SampleEntity.cs"),
$@"namespace {projectName}.Core.Entities;
public class SampleEntity
{{
    public int Id {{ get; set; }}
    public string Name {{ get; set; }}
}}");

            // HomeController
            var webControllers = Path.Combine(projectRoot, "Web", "Controllers");
            Directory.CreateDirectory(webControllers);
            File.WriteAllText(Path.Combine(webControllers, "HomeController.cs"),
$@"using Microsoft.AspNetCore.Mvc;

namespace {projectName}.Controllers;
public class HomeController : Controller
{{
    public IActionResult Index() => View();
}}");
        }

        private void GenerateCleanArchitecture(string projectRoot, string projectName, string efVersion, string database, string framework)
        {
            RunCmd($"dotnet new classlib -n Domain --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new classlib -n Application --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new classlib -n Infrastructure --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new mvc -n Web --framework {framework} --no-restore", projectRoot);

            RunCmd("dotnet sln add Domain Application Infrastructure Web", projectRoot);
            RunCmd("dotnet add Application reference Domain", projectRoot);
            RunCmd("dotnet add Infrastructure reference Application Domain", projectRoot);
            RunCmd("dotnet add Web reference Application Infrastructure", projectRoot);

            RunCmd($"dotnet add Infrastructure package {GetDbPackage(database)} --version {efVersion} --no-restore", projectRoot);

            RemovePlaceholderFiles(projectRoot);

            // Domain entity
            var domainFolder = Path.Combine(projectRoot, "Domain", "Entities");
            Directory.CreateDirectory(domainFolder);
            File.WriteAllText(Path.Combine(domainFolder, "ProjectBlueprint.cs"),
$@"namespace {projectName}.Entities;
public class ProjectBlueprint
{{
    public int Id {{ get; set; }}
    public string Name {{ get; set; }}
    public void Validate()
    {{
        if (string.IsNullOrEmpty(Name))
            throw new System.InvalidOperationException(""Project name cannot be empty"");
    }}
}}");

            // Application interface
            var appFolder = Path.Combine(projectRoot, "Application", "Interfaces");
            Directory.CreateDirectory(appFolder);
            File.WriteAllText(Path.Combine(appFolder, "IProjectRepository.cs"),
$@"using {projectName}.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace {projectName}.Interfaces;
public interface IProjectRepository
{{
    Task<List<ProjectBlueprint>> GetAllAsync();
    Task AddAsync(ProjectBlueprint project);
}}");

            // CQRS folders
            Directory.CreateDirectory(Path.Combine(projectRoot, "Application", "Commands"));
            Directory.CreateDirectory(Path.Combine(projectRoot, "Application", "Queries"));

            // Infrastructure repository
            var infraFolder = Path.Combine(projectRoot, "Infrastructure", "Repositories");
            Directory.CreateDirectory(infraFolder);
            File.WriteAllText(Path.Combine(infraFolder, "ProjectRepository.cs"),
$@"using {projectName}.Interfaces;
using {projectName}.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace {projectName}.Repositories;
public class ProjectRepository : IProjectRepository
{{
    private readonly List<ProjectBlueprint> _store = new();
    public Task<List<ProjectBlueprint>> GetAllAsync() => Task.FromResult(_store);
    public Task AddAsync(ProjectBlueprint project)
    {{
        _store.Add(project);
        return Task.CompletedTask;
    }}
}}");

            // Web controller
            var webControllers = Path.Combine(projectRoot, "Web", "Controllers");
            Directory.CreateDirectory(webControllers);
            File.WriteAllText(Path.Combine(webControllers, "ProjectController.cs"),
$@"using {projectName}.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace {projectName}.Controllers;
public class ProjectController : Controller
{{
    private readonly IProjectRepository _repo;
    public ProjectController(IProjectRepository repo) => _repo = repo;

    public async Task<IActionResult> Index()
    {{
        var projects = await _repo.GetAllAsync();
        return View(projects);
    }}
}}");

            File.WriteAllText(Path.Combine(webControllers, "HomeController.cs"),
$@"using Microsoft.AspNetCore.Mvc;

namespace {projectName}.Controllers;
public class HomeController : Controller
{{
    public IActionResult Index() => View();
}}");
        }

        private void GenerateMVVM(string projectRoot, string projectName, string framework)
        {
            RunCmd($"dotnet new classlib -n ViewModels --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new mvc -n Web --framework {framework} --no-restore", projectRoot);
            RunCmd("dotnet sln add ViewModels Web", projectRoot);
            RunCmd("dotnet add Web reference ViewModels", projectRoot);

            Directory.CreateDirectory(Path.Combine(projectRoot, "ViewModels", "Home"));
            File.WriteAllText(Path.Combine(projectRoot, "ViewModels", "Home", "IndexViewModel.cs"),
$@"namespace {projectName}.ViewModels.Home;
public class IndexViewModel
{{
    public string Title {{ get; set; }} = ""Welcome to MVVM"";
    public string Message {{ get; set; }} = ""This is a ViewModel generated project."";
}}");
        }

        private void GenerateMicroservices(string projectRoot, string projectName, string efVersion, string database, string framework)
        {
            string[] services = { "Identity.Service", "Catalog.Service", "Ordering.Service", "Gateway.API" };
            foreach (var svc in services)
            {
                RunCmd($"dotnet new webapi -n {svc} --framework {framework} --no-restore", projectRoot);
                RunCmd($"dotnet sln add {svc}", projectRoot);
                if (svc != "Gateway.API")
                {
                    RunCmd($"dotnet add {svc} package {GetDbPackage(database)} --version {efVersion} --no-restore", projectRoot);
                }
            }

            RunCmd($"dotnet new classlib -n Shared.Core --framework {framework} --no-restore", projectRoot);
            RunCmd("dotnet sln add Shared.Core", projectRoot);
            foreach (var svc in services)
            {
                RunCmd($"dotnet add {svc} reference Shared.Core", projectRoot);
            }
        }

        private void GenerateHexagonal(string projectRoot, string projectName, string efVersion, string database, string framework)
        {
            RunCmd($"dotnet new classlib -n Domain --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new classlib -n Application --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new classlib -n Infrastructure --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new webapi -n API --framework {framework} --no-restore", projectRoot);

            RunCmd("dotnet sln add Domain Application Infrastructure API", projectRoot);

            RunCmd("dotnet add Application reference Domain", projectRoot);
            RunCmd("dotnet add Infrastructure reference Application Domain", projectRoot);
            RunCmd("dotnet add API reference Application Infrastructure", projectRoot);

            RunCmd($"dotnet add Infrastructure package {GetDbPackage(database)} --version {efVersion} --no-restore", projectRoot);

            // Hexagonal structure: Ports in Application, Adapters in Infrastructure
            Directory.CreateDirectory(Path.Combine(projectRoot, "Application", "Ports", "In"));
            Directory.CreateDirectory(Path.Combine(projectRoot, "Application", "Ports", "Out"));
            Directory.CreateDirectory(Path.Combine(projectRoot, "Infrastructure", "Adapters", "Persistence"));
            Directory.CreateDirectory(Path.Combine(projectRoot, "Infrastructure", "Adapters", "ExternalServices"));
        }

        private void GenerateCQRS(string projectRoot, string projectName, string efVersion, string database, string framework)
        {
            RunCmd($"dotnet new classlib -n Domain --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new classlib -n Application --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new classlib -n Infrastructure --framework {framework} --no-restore", projectRoot);
            RunCmd($"dotnet new webapi -n API --framework {framework} --no-restore", projectRoot);

            RunCmd("dotnet sln add Domain Application Infrastructure API", projectRoot);
            RunCmd("dotnet add Application reference Domain", projectRoot);
            RunCmd("dotnet add Infrastructure reference Application", projectRoot);
            RunCmd("dotnet add API reference Application Infrastructure", projectRoot);

            RunCmd($"dotnet add Infrastructure package {GetDbPackage(database)} --version {efVersion} --no-restore", projectRoot);

            // CQRS Structure in Application
            var appPath = Path.Combine(projectRoot, "Application");
            Directory.CreateDirectory(Path.Combine(appPath, "Commands"));
            Directory.CreateDirectory(Path.Combine(appPath, "Queries"));
            Directory.CreateDirectory(Path.Combine(appPath, "Handlers"));
            Directory.CreateDirectory(Path.Combine(appPath, "DTOs"));
        }

        private void GenerateMVC(string projectRoot, string projectName, string framework)
        {
            RunCmd($"dotnet new mvc -n Web --framework {framework} --no-restore", projectRoot);
            RunCmd("dotnet sln add Web", projectRoot);

            var webControllers = Path.Combine(projectRoot, "Web", "Controllers");
            Directory.CreateDirectory(webControllers);
            File.WriteAllText(Path.Combine(webControllers, "HomeController.cs"),
$@"using Microsoft.AspNetCore.Mvc;

namespace {projectName}.Controllers;
public class HomeController : Controller
{{
    public IActionResult Index() => View();
}}");
        }

        private void RemovePlaceholderFiles(string projectRoot)
        {
            var classFiles = Directory.GetFiles(projectRoot, "Class1.cs", SearchOption.AllDirectories);
            foreach (var file in classFiles) File.Delete(file);
        }

        #endregion

        private void RunCmd(string command, string workingDir)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception(
                    $"❌ Command failed:\n{command}\n\nERROR:\n{error}\n\nOUTPUT:\n{output}"
                );
            }
        }

        private string GetEFCoreVersion(string netVersion)
        {
            return netVersion switch
            {
                "6.0" => "6.0.36",
                "7.0" => "7.0.20",
                "8.0" => "8.0.12",
                "9.0" => "9.0.1",
                "10.0" => "10.0.0-preview.1.25080.9",
                _ => "8.0.12"
            };
        }

        private string GetDbPackage(string database)
        {
            return database switch
            {
                "PostgreSQL" => "Npgsql.EntityFrameworkCore.PostgreSQL",
                "MySQL" => "Pomelo.EntityFrameworkCore.MySql",
                "SQLite" => "Microsoft.EntityFrameworkCore.Sqlite",
                _ => "Microsoft.EntityFrameworkCore.SqlServer"
            };
        }
    }
}
