using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Employee
    {
        [Key]
        public int Employee_ID { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        [ForeignKey(nameof(Department))]
        public int Department_ID { get; set; } // FK
        public Department? Department { get; set; }
        [ForeignKey(nameof(JobRole))]
        public int? JobRole_ID { get; set; } // FK to JobRole
        public JobRole? JobRole { get; set; }
        public Payroll? Payroll { get; set; } // 1:1 relationship with Payroll
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Leave> Leaves { get; set; } = new List<Leave>();
        public ICollection<Employee_Project> Employee_Projects { get; set; } = new List<Employee_Project>();
        public ICollection<Employee_Training> Employee_Trainings { get; set; } = new List<Employee_Training>();
        public ICollection<Benefit>? Benefits { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
