using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class PersonalGoal
    {
        [Key]
        public int PersonalGoal_ID { get; set; }
        [ForeignKey(nameof(Employee))]
        public int Employee_ID { get; set; }
        public Employee? Employee { get; set; }
        [Required]
        public string Goal { get; set; } = string.Empty;
        public DateTime TargetDate { get; set; }
        public bool IsAchieved { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
