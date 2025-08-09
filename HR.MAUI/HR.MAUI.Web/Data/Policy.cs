using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Policy
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Policy_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
        public string Version { get; set; } = "1.0";
        public DateTime EffectiveDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ApprovalNotes { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
