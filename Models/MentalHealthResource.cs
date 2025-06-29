namespace HR.Models
{
    public class MentalHealthResourceRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class MentalHealthResourceResponse
    {
        public int MentalHealthResource_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
    }
}
