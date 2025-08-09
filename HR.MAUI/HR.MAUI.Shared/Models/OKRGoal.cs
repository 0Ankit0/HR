namespace HR.Models
{
    public class OKRGoalRequest
    {
        public int Employee_ID { get; set; }
        public string Objective { get; set; } = string.Empty;
        public string KeyResults { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class OKRGoalResponse
    {
        public int OKRGoal_ID { get; set; }
        public int Employee_ID { get; set; }
        public string Objective { get; set; } = string.Empty;
        public string KeyResults { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
