namespace HR.Models
{
    public class PolicyRequest
    {
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
    }

    public class PolicyResponse
    {
        public int Policy_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ApprovalNotes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string Priority { get; set; } = "Medium";
    }

    public class PolicyListResponse
    {
        public List<PolicyResponse> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
