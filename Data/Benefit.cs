using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Benefit
    {
        [Key]
        public int Benefit_ID { get; set; }
        [Required]
        public string BenefitType { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public DateTime? EndDate { get; set; }
        [ForeignKey(nameof(Employee))]
        public int Employee_ID { get; set; }
        public Employee? Employee { get; set; }
        // Soft delete and audit fields
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
