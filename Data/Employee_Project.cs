using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Employee_Project
    {
        public int Employee_Project_ID { get; set; }
        [ForeignKey(nameof(Employee))]
        public int Employee_ID { get; set; }
        public Employee? Employee { get; set; }
        [ForeignKey(nameof(Project))]
        public int Project_ID { get; set; }
        public Project? Project { get; set; }
        public DateTime Assignment_Date { get; set; }
        public string Role_on_Project { get; set; } = string.Empty;
    }
}
