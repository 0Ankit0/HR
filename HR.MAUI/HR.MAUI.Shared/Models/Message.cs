namespace HR.Models
{
    public class MessageRequest
    {
        public int Sender_ID { get; set; }
        public int Recipient_ID { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class MessageResponse
    {
        public int Message_ID { get; set; }
        public int Sender_ID { get; set; }
        public string? SenderName { get; set; }
        public int Recipient_ID { get; set; }
        public string? RecipientName { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}
