namespace HR.Models
{
    public class PayrollRequest
    {
        public int Employee_ID { get; set; }
        public decimal Salary { get; set; }
        public DateTime PayDate { get; set; }
    }

    public class PayrollResponse
    {
        public int Payroll_ID { get; set; }
        public int Employee_ID { get; set; }
        public decimal Salary { get; set; }
        public DateTime PayDate { get; set; }
    }
}
