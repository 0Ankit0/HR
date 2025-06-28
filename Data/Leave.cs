using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Leave
    {
        [Key]
        public int Leave_ID { get; set; }
        [ForeignKey(nameof(Employee))]
        public int Employee_ID { get; set; } // FK to Employee
        public Employee? Employee { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
