using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Training
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Training_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public ICollection<Employee_Training> Employee_Trainings { get; set; } = new List<Employee_Training>();
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
