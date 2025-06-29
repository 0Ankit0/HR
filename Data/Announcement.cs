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
    }
}
