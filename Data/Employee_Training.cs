using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Employee_Training
    {
        public int Employee_Training_ID { get; set; }
        [ForeignKey(nameof(Employee))]
        public int Employee_ID { get; set; }
        public Employee? Employee { get; set; }
        [ForeignKey(nameof(Training))]
        public int Training_ID { get; set; }
        public Training? Training { get; set; }
        public DateTime? Completion_Date { get; set; }
        public double? Score { get; set; }
    }
}
