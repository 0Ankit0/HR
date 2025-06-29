namespace HR.Models
{
    public class AwardRequest
    {
        public string Title { get; set; } = string.Empty;
        public int Employee_ID { get; set; }
    }

    public class AwardResponse
    {
        public int Award_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateAwarded { get; set; }
        public int Employee_ID { get; set; }
    }
}
