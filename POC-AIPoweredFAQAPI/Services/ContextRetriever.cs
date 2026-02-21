using POC_AIPoweredFAQAPI.Interfaces;
using POC_AIPoweredFAQAPI.Models;
using POC_AIPoweredFAQAPI.IRepositories;

namespace POC_AIPoweredFAQAPI.Services;

public class ContextRetriever : IContextRetriever
{
    private readonly IEmbeddingClient _embeddingClient;
    private readonly IKnowledgeBaseRepository _kb;
    private readonly OpenAiOptions _options;

    public ContextRetriever(IEmbeddingClient embeddingClient, IKnowledgeBaseRepository kb, Microsoft.Extensions.Options.IOptions<OpenAiOptions> options)
    {
        _embeddingClient = embeddingClient;
        _kb = kb;
        _options = options.Value;
    }

    public async Task<IList<FaqItem>> RetrieveAsync(string question, CancellationToken cancellationToken = default)
    {
        var req = new OpenAiEmbeddingRequest { Model = _options.EmbeddingModel ?? string.Empty, Input = new List<string> { question } };
        var resp = await _embeddingClient.CreateEmbeddingAsync(req, cancellationToken);
        var emb = resp.Data.FirstOrDefault()?.Embedding ?? Array.Empty<double>().ToList();
        var results = await _kb.QueryByEmbeddingAsync(emb, 5, cancellationToken);
        return results;
    }
}
