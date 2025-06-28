using System;
using System.ComponentModel.DataAnnotations;

namespace HR.Data
{
    public class WellnessProgram
    {
        [Key]
        public int WellnessProgram_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
