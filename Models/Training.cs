namespace HR.Models
{
    public class TrainingRequest
    {
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class TrainingResponse
    {
        public int Training_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
