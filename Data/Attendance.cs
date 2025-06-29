using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Attendance
    {
        [Key]
        public int Attendance_ID { get; set; }
        [ForeignKey(nameof(Employee))]
        public int Employee_ID { get; set; } // FK to Employee
        public Employee? Employee { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty; // Present/Absent
        public TimeSpan TimeIn { get; set; }
        public TimeSpan TimeOut { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
