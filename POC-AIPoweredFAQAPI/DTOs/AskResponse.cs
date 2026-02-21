namespace POC_AIPoweredFAQAPI.DTOs
{
    public class AskResponse
    {
        public string Answer { get; set; } = string.Empty;
        public string Confidence { get; set; } = "low";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
