namespace HR.Models
{
    public class JobRoleRequest
    {
        public string Role_Name { get; set; } = string.Empty;
    }

    public class JobRoleResponse
    {
        public int JobRole_ID { get; set; }
        public string Role_Name { get; set; } = string.Empty;
        public string Role_Description { get; set; } = string.Empty;
    }
}
