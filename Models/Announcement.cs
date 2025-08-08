namespace HR.Models
{
    public class AnnouncementRequest
    {
        public int? Announcement_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? DatePosted { get; set; }
        public bool IsPinned { get; set; }
    }

    public class AnnouncementResponse
    {
        public int Announcement_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime? DateCreated { get; set; }
        public string? CreatedBy { get; set; }
        public string? AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ViewCount { get; set; }
        public bool IsPinned { get; set; }
    }

    public class AnnouncementListResponse
    {
        public List<AnnouncementResponse> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
