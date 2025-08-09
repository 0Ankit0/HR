using System.ComponentModel.DataAnnotations;

namespace HR.Models
{
    public class JobRoleRequest
    {
        [Required(ErrorMessage = "Job title is required")]
        [StringLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Level is required")]
        public string Level { get; set; } = string.Empty;

        public string Status { get; set; } = "Active";

        [Range(0, 30, ErrorMessage = "Experience must be between 0 and 30 years")]
        public int MinExperience { get; set; }

        public string SalaryRange { get; set; } = string.Empty;

        public List<string> KeyResponsibilities { get; set; } = new();

        public List<string> RequiredSkills { get; set; } = new();

        // Mapping properties for API compatibility
        public string Role_Name
        {
            get => Title;
            set => Title = value;
        }

        public string Role_Description
        {
            get => Description;
            set => Description = value;
        }
    }

    public class JobRoleResponse
    {
        public int JobRole_ID { get; set; }
        public string Role_Name { get; set; } = string.Empty;
        public string Role_Description { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int MinExperience { get; set; }
        public string SalaryRange { get; set; } = string.Empty;
        public List<string> KeyResponsibilities { get; set; } = new();
        public List<string> RequiredSkills { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Computed properties for compatibility with Razor component
        public int JobRoleID => JobRole_ID;
        public string Title => Role_Name;
        public string Description => Role_Description;
        public DateTime LastModified => UpdatedAt ?? CreatedAt;
    }
}
