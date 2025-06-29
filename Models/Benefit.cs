namespace HR.Models
{
    public class BenefitRequest
    {
        public string BenefitType { get; set; } = string.Empty;
        public int Employee_ID { get; set; }
    }

    public class BenefitResponse
    {
        public int Benefit_ID { get; set; }
        public string BenefitType { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public int Employee_ID { get; set; }
    }
}
