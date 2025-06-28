using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class PerformanceReview
    {
        [Key]
        public int PerformanceReview_ID { get; set; }
        [ForeignKey(nameof(Employee))]
        public int Employee_ID { get; set; }
        public Employee? Employee { get; set; }
        public DateTime ReviewDate { get; set; }
        public string Reviewer { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public int Score { get; set; }
    }
}
