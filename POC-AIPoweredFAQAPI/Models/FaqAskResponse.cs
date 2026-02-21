namespace POC_AIPoweredFAQAPI.Models;

public class FaqAskResponse
{
    public string Answer { get; set; } = string.Empty;
    public string Confidence { get; set; } = "0";
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
