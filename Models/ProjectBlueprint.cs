using System;

namespace DotNetBlueprint.Models
{
    public class ProjectBlueprint
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } ="";
        public string Architecture { get; set; } = "";
         public string DotNetVersion { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
