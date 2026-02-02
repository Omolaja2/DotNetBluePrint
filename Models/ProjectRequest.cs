using System.ComponentModel.DataAnnotations;

namespace DotNetBlueprint.Models
{
    public class ProjectRequest
    {
        [Required]
        public string ProjectName { get; set; } = "";

        [Required]
        public string NetVersion { get; set;} = "";
        public ArchitectureType Architecture { get; set; }
        public DatabaseType Database { get; set; }
    }
}
