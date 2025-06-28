using System;
using System.ComponentModel.DataAnnotations;

namespace HR.Data
{
    public class DEIResource
    {
        [Key]
        public int DEIResource_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
