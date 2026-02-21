namespace POC_AIPoweredFAQAPI.Models;

public class OpenAiChatResponse
{
    public string? Id { get; set; }
    public string? Object { get; set; }
    public List<OpenAiChatChoice> Choices { get; set; } = new();
    public OpenAiChatUsage? Usage { get; set; }
}
