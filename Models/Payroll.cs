namespace HR.Models
{
    public class PayrollRequest
    {
        public int Payroll_ID { get; set; }
        public int Employee_ID { get; set; }
        public decimal Salary { get; set; }
        public DateTime PayDate { get; set; }
        public string PayFrequency { get; set; } = string.Empty;
        public DateTime PayPeriodStart { get; set; }
        public DateTime PayPeriodEnd { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal GrossPay { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
    }

    public class PayrollResponse
    {
        public int Payroll_ID { get; set; }
        public int Employee_ID { get; set; }
        public decimal Salary { get; set; }
        public DateTime PayDate { get; set; }
        public string PayFrequency { get; set; } = string.Empty;
        public DateTime PayPeriodStart { get; set; }
        public DateTime PayPeriodEnd { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal GrossPay { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
    }
}
