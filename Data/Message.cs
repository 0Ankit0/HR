using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Message
    {
        [Key]
        public int Message_ID { get; set; }
        [ForeignKey(nameof(Sender))]
        public int Sender_ID { get; set; }
        public Employee? Sender { get; set; }
        [ForeignKey(nameof(Recipient))]
        public int Recipient_ID { get; set; }
        public Employee? Recipient { get; set; }
        [Required]
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        // Soft delete and audit fields
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
