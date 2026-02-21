using POC_AIPoweredFAQAPI.Interfaces;
using POC_AIPoweredFAQAPI.IRepositories;
using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Services;

public class FaqIngestionService : IFaqIngestionService
{
    private readonly IEmbeddingClient _embeddingClient;
    private readonly IKnowledgeBaseIngestionRepository _ingestRepo;
    private readonly OpenAiOptions _options;

    public FaqIngestionService(IEmbeddingClient embeddingClient, IKnowledgeBaseIngestionRepository ingestRepo, Microsoft.Extensions.Options.IOptions<OpenAiOptions> options)
    {
        _embeddingClient = embeddingClient;
        _ingestRepo = ingestRepo;
        _options = options.Value;
    }

    public async Task IngestAsync(FaqIngestRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items == null || !request.Items.Any()) return;

        foreach (var item in request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Question) || string.IsNullOrWhiteSpace(item.Answer)) continue;
            var embReq = new OpenAiEmbeddingRequest { Model = _options.EmbeddingModel ?? string.Empty, Input = new List<string> { item.Question } };
            var embResp = await _embeddingClient.CreateEmbeddingAsync(embReq, cancellationToken);
            var emb = embResp.Data.FirstOrDefault()?.Embedding ?? Array.Empty<double>().ToList();
            await _ingestRepo.UpsertAsync(item, emb, cancellationToken);
        }
    }
}
