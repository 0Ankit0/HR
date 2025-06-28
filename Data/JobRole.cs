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
    }
}