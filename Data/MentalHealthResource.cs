using System;
using System.ComponentModel.DataAnnotations;

namespace HR.Data
{
    public class MentalHealthResource
    {
        [Key]
        public int MentalHealthResource_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
    }
}
