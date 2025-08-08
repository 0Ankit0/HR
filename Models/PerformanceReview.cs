using System.ComponentModel.DataAnnotations;

namespace HR.Models
{
    public class PerformanceReviewRequest
    {
        [Required]
        public int Employee_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Reviewer { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Comments { get; set; } = string.Empty;

        [Required]
        public DateTime ReviewDate { get; set; }

        [Range(1, 10)]
        public int Score { get; set; }

        [StringLength(50)]
        public string ReviewType { get; set; } = "Annual"; // Annual, Quarterly, Monthly

        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Published

        [StringLength(1000)]
        public string Goals { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Achievements { get; set; } = string.Empty;

        [StringLength(1000)]
        public string AreasForImprovement { get; set; } = string.Empty;

        public DateTime? NextReviewDate { get; set; }
    }

    public class PerformanceReviewResponse
    {
        public int PerformanceReview_ID { get; set; }
        public int Employee_ID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string JobRoleName { get; set; } = string.Empty;
        public string Reviewer { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        public int Score { get; set; }
        public string ReviewType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Goals { get; set; } = string.Empty;
        public string Achievements { get; set; } = string.Empty;
        public string AreasForImprovement { get; set; } = string.Empty;
        public DateTime? NextReviewDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }

    public class PerformanceReviewStats
    {
        public int TotalReviews { get; set; }
        public int CompletedReviews { get; set; }
        public int PendingReviews { get; set; }
        public int OverdueReviews { get; set; }
        public double AverageScore { get; set; }
        public List<ReviewsByDepartment> ReviewsByDepartment { get; set; } = new();
        public List<ReviewsByMonth> ReviewsByMonth { get; set; } = new();
    }

    public class ReviewsByDepartment
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AverageScore { get; set; }
    }

    public class ReviewsByMonth
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AverageScore { get; set; }
    }
}
