namespace POC_AIPoweredFAQAPI.Models;

public class OpenAiChatUsage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}
