namespace HR.Models
{
    public class GoalRequest
    {
        public int Employee_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public string KeyResults { get; set; } = string.Empty;
        public string Type { get; set; } = "Personal"; // Personal, Team, Company
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Progress { get; set; } = 0; // 0-100
    }

    public class GoalResponse
    {
        public int OKRGoal_ID { get; set; }
        public int Employee_ID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public string KeyResults { get; set; } = string.Empty;
        public string Type { get; set; } = "Personal";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Progress { get; set; } = 0;
        public string Status { get; set; } = "Not Started"; // Not Started, In Progress, Completed, On Hold
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GoalStatusUpdateRequest
    {
        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
        public bool IsCompleted { get; set; }
    }
}
