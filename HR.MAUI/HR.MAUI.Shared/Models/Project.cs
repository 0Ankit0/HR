namespace HR.Models
{
    public class ProjectRequest
    {
        public int? Project_ID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Project_Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }
        public DateTime Deadline { get; set; }
        public decimal Budget { get; set; }
        public decimal? ActualCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int? Manager_ID { get; set; }
        public int? ManagerId { get; set; }
        public decimal Progress { get; set; }
    }

    public class ProjectResponse
    {
        public int Project_ID { get; set; }
        public string Project_Name { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime Deadline { get; set; }
        public decimal Budget { get; set; }
        public decimal? ActualCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int? ManagerId { get; set; }
        public int? Manager_ID { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public int TeamSize { get; set; }
        public decimal Progress { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ProjectListResponse
    {
        public List<ProjectResponse> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
