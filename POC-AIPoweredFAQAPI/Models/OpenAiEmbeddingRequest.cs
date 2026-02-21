namespace POC_AIPoweredFAQAPI.Models;

public class OpenAiEmbeddingRequest
{
    public List<string> Input { get; set; } = new();
    public string Model { get; set; } = string.Empty;
}
