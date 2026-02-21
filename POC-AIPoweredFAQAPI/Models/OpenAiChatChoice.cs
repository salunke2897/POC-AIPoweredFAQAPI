namespace POC_AIPoweredFAQAPI.Models;

public class OpenAiChatChoice
{
    public int Index { get; set; }
    public OpenAiChatMessage Message { get; set; } = new();
    public string? FinishReason { get; set; }
}
