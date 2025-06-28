using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HR.Data
{
    public class Project
    {
        [Key]
        public int Project_ID { get; set; }
        public string Project_Name { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public decimal Budget { get; set; }
        public ICollection<Employee_Project> Employee_Projects { get; set; } = new List<Employee_Project>();
    }
}
