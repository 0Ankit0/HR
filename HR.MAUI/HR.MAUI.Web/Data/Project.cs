using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Project
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Project_ID { get; set; }
        public string Project_Name { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public decimal Budget { get; set; }
        public ICollection<Employee_Project> Employee_Projects { get; set; } = new List<Employee_Project>();
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
