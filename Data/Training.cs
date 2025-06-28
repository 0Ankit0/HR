using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HR.Data
{
    public class Training
    {
        [Key]
        public int Training_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public ICollection<Employee_Training> Employee_Trainings { get; set; } = new List<Employee_Training>();
    }
}
