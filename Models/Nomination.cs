namespace HR.Models
{
    public class NominationRequest
    {
        public int Employee_ID { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class NominationResponse
    {
        public int Nomination_ID { get; set; }
        public int Employee_ID { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime DateNominated { get; set; }
        public bool IsAwarded { get; set; }
    }
}
