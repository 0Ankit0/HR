namespace HR.Models
{
    public class EmployeeTrainingRequest
    {
        public int Employee_ID { get; set; }
        public int Training_ID { get; set; }
    }

    public class EmployeeTrainingResponse
    {
        public int Employee_Training_ID { get; set; }
        public int Employee_ID { get; set; }
        public int Training_ID { get; set; }
        public DateTime? Completion_Date { get; set; }
        public double? Score { get; set; }
    }
}
