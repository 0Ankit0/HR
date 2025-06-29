using System.ComponentModel.DataAnnotations;

namespace HR.Data
{
    public class JobRole
    {
        [Key]
        public int JobRole_ID { get; set; }
        [Required]
        public string Role_Name { get; set; } = string.Empty;
        public string Role_Description { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}