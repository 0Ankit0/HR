using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Payroll
    {
        [Key]
        public int Payroll_ID { get; set; }
        [ForeignKey(nameof(Employee))]
        public int Employee_ID { get; set; } // FK to Employee
        public Employee? Employee { get; set; }
        public decimal Salary { get; set; }
        public DateTime Payment_Date { get; set; }
        public decimal Bonus { get; set; }
        public decimal NetPay { get; set; }
    }
}
