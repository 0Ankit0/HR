using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR.Data
{
    public class Application
    {
        [Key]
        public int Application_ID { get; set; }
        [ForeignKey(nameof(JobPosting))]
        public int JobPosting_ID { get; set; }
        public JobPosting? JobPosting { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
