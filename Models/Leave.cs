namespace HR.Models
{
    public class LeaveRequest
    {
        public int Employee_ID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveType { get; set; } = string.Empty;
    }

    public class LeaveResponse
    {
        public int Leave_ID { get; set; }
        public int Employee_ID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
