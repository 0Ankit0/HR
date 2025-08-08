namespace HR.Models
{
    public class EmployeeTrainingRequest
    {
        public int? Employee_Training_ID { get; set; }
        public int Employee_ID { get; set; }
        public int Training_ID { get; set; }
        public DateTime? Completion_Date { get; set; }
        public double? Score { get; set; }
    }

    public class EmployeeTrainingResponse
    {
        public int Employee_Training_ID { get; set; }
        public int Employee_ID { get; set; }
        public string? EmployeeName { get; set; }
        public int Training_ID { get; set; }
        public string? TrainingTitle { get; set; }
        public DateTime? Completion_Date { get; set; }
        public double? Score { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
    }

    public class EmployeeTrainingCompletionRequest
    {
        public DateTime? CompletionDate { get; set; }
        public double? Score { get; set; }
    }

    public class BulkEmployeeTrainingRequest
    {
        public int TrainingId { get; set; }
        public List<int> EmployeeIds { get; set; } = new();
    }
}
