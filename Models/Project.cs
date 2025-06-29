namespace HR.Models
{
    public class ProjectRequest
    {
        public string Project_Name { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public decimal Budget { get; set; }
    }

    public class ProjectResponse
    {
        public int Project_ID { get; set; }
        public string Project_Name { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public decimal Budget { get; set; }
    }
}
