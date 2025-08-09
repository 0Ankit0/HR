using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class JobRole
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int JobRole_ID { get; set; }
        [Required]
        public string Role_Name { get; set; } = string.Empty;
        public string Role_Description { get; set; } = string.Empty;
        [Required]
        public string Department { get; set; } = string.Empty;
        [Required]
        public string Level { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public int MinExperience { get; set; } = 0;
        public string SalaryRange { get; set; } = string.Empty;
        public string KeyResponsibilities { get; set; } = string.Empty; // JSON string
        public string RequiredSkills { get; set; } = string.Empty; // JSON string
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}