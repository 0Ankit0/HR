using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Nomination
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Nomination_ID { get; set; }
        [ForeignKey(nameof(Employee))]
        public int Employee_ID { get; set; }
        public Employee? Employee { get; set; }
        [Required]
        public string Reason { get; set; } = string.Empty;
        public DateTime DateNominated { get; set; }
        public bool IsAwarded { get; set; }
        public string? NominatedBy { get; set; }
        // Soft delete and audit fields
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
