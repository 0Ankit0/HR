namespace HR.Models
{
    public class LeaveRequest
    {
        public int Employee_ID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool IsEmergency { get; set; } = false;
    }

    public class LeaveResponse
    {
        public int Leave_ID { get; set; }
        public int Employee_ID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; } // days
        public string LeaveType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool IsEmergency { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class LeaveApprovalRequest
    {
        public string Status { get; set; } = string.Empty; // "Approved", "Rejected"
        public string? ApprovalComments { get; set; }
    }
}
