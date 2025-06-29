namespace HR.Models
{
    public class GrievanceRequest
    {
        public int Employee_ID { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class GrievanceResponse
    {
        public int Grievance_ID { get; set; }
        public int Employee_ID { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
