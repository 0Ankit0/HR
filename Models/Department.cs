namespace HR.Models
{
    public class DepartmentRequest
    {
        public string Department_Name { get; set; } = string.Empty;
        public string Department_Location { get; set; } = string.Empty;
        public int? ManagerID { get; set; }
    }

    public class DepartmentResponse
    {
        public int Department_ID { get; set; }
        public string Department_Name { get; set; } = string.Empty;
        public string Department_Location { get; set; } = string.Empty;
        public int? ManagerID { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
    }
}
