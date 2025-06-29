namespace HR.Models
{
    public class IncidentRequest
    {
        public string Description { get; set; } = string.Empty;
    }

    public class IncidentResponse
    {
        public int Incident_ID { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
