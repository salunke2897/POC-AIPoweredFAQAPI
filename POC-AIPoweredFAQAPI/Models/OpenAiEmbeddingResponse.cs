namespace POC_AIPoweredFAQAPI.Models;

public class OpenAiEmbeddingResponse
{
    public IList<OpenAiEmbeddingData> Data { get; set; } = new List<OpenAiEmbeddingData>();
}
