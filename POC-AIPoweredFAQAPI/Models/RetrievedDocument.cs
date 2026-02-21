namespace POC.AIPoweredFAQAPI.Models
{
    public class RetrievedDocument
    {
        public Guid Id { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}
