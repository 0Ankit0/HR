namespace HR.Models
{
    public class EmployeeProjectRequest
    {
        public int Employee_ID { get; set; }
        public int Project_ID { get; set; }
    }

    public class EmployeeProjectResponse
    {
        public int Employee_Project_ID { get; set; }
        public int Employee_ID { get; set; }
        public int Project_ID { get; set; }
        public DateTime Assignment_Date { get; set; }
        public string Role_on_Project { get; set; } = string.Empty;
    }
}
