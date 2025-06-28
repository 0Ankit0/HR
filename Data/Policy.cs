using System;
using System.ComponentModel.DataAnnotations;

namespace HR.Data
{
    public class Policy
    {
        [Key]
        public int Policy_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
    }
}
