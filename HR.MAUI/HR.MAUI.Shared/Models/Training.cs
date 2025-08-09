using System.ComponentModel.DataAnnotations;

namespace HR.Models
{
    public class TrainingRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(100)]
        public string Instructor { get; set; } = string.Empty;

        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int MaxAttendees { get; set; } = 50;

        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, In Progress, Completed, Cancelled

        [StringLength(50)]
        public string TrainingType { get; set; } = "Classroom"; // Classroom, Online, Hybrid

        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        public decimal Cost { get; set; } = 0;

        [StringLength(500)]
        public string Prerequisites { get; set; } = string.Empty;

        [StringLength(500)]
        public string LearningObjectives { get; set; } = string.Empty;

        public bool IsMandatory { get; set; } = false;
    }

    public class TrainingResponse
    {
        public int Training_ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? EndDate { get; set; }
        public string Instructor { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int MaxAttendees { get; set; }
        public int CurrentAttendees { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TrainingType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string Prerequisites { get; set; } = string.Empty;
        public string LearningObjectives { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public List<string> EnrolledEmployees { get; set; } = new();
    }

    public class TrainingStats
    {
        public int TotalTrainings { get; set; }
        public int ScheduledTrainings { get; set; }
        public int CompletedTrainings { get; set; }
        public int CancelledTrainings { get; set; }
        public int TotalAttendees { get; set; }
        public decimal TotalCost { get; set; }
        public List<TrainingByCategory> TrainingByCategory { get; set; } = new();
        public List<TrainingByMonth> TrainingByMonth { get; set; } = new();
    }

    public class TrainingByCategory
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public int TotalAttendees { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class TrainingByMonth
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
        public int TotalAttendees { get; set; }
        public decimal TotalCost { get; set; }
    }
}
