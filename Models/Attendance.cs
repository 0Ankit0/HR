namespace HR.Models
{
    public class AttendanceRequest
    {
        public int Employee_ID { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public TimeSpan TimeIn { get; set; }
        public TimeSpan TimeOut { get; set; }
    }

    public class AttendanceResponse
    {
        public int Attendance_ID { get; set; }
        public int Employee_ID { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public TimeSpan TimeIn { get; set; }
        public TimeSpan TimeOut { get; set; }
    }
}
