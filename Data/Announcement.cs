using System;
using System.ComponentModel.DataAnnotations;

namespace HR.Data
{
    public class Announcement
    {
        [Key]
        public int Announcement_ID { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; }
        // Soft delete and audit fields
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
