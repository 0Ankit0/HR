namespace HR.Models
{
    public class EmployeeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Department_ID { get; set; }
    }

    public class EmployeeResponse
    {
        public int Employee_ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public int Department_ID { get; set; }
        public int? JobRole_ID { get; set; }
    }
}
