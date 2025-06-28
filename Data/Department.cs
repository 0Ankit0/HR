using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Department
    {
        [Key]
        public int Department_ID { get; set; }
        [Required]
        public string Department_Name { get; set; } = string.Empty;
        public string Department_Location { get; set; } = string.Empty;
        [ForeignKey(nameof(Manager))]
        public int? ManagerID { get; set; } // FK to Employee, nullable
        public Employee? Manager { get; set; }
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
