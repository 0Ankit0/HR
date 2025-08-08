namespace HR.Models
{
    public class PersonalGoalRequest
    {
        public int Employee_ID { get; set; }
        public string Goal { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCompleted { get; set; } = false;
    }

    public class PersonalGoalResponse
    {
        public int PersonalGoal_ID { get; set; }
        public int Employee_ID { get; set; }
        public string Goal { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
