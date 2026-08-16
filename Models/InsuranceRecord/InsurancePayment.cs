namespace CarCareTracker.Models
{
    public class InsurancePayment
    {
        public string Date { get; set; } = DateTime.Now.ToShortDateString();
        public decimal Amount { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
