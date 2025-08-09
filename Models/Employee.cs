namespace HR.Models
{
    public class Employee
    {
        public int Employee_ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public int Department_ID { get; set; }
        public int? JobRole_ID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class EmployeeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; } = DateTime.Now;
        public int Department_ID { get; set; }
        public int? JobRole_ID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class EmployeeResponse
    {
        public int Employee_ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public int Department_ID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int? JobRole_ID { get; set; }
        public string JobRoleName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class EmployeeWithDetailsResponse : EmployeeResponse
    {
        public decimal? Salary { get; set; }
        public int LeaveBalance { get; set; }
        public DateTime LastLogin { get; set; }
        public string Status { get; set; } = "Active";
    }
}