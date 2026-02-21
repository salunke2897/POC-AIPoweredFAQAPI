namespace POC_AIPoweredFAQAPI.Models;

public class OpenAiOptions
{
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
    public string? Model { get; set; }
    public string? EmbeddingsEndpoint { get; set; }
    public string? EmbeddingModel { get; set; }
    public int MaxTokens { get; set; } = 1024;
    public double Temperature { get; set; } = 0.2;
}
