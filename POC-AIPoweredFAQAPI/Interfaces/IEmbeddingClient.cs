using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Interfaces;

public interface IEmbeddingClient
{
    Task<OpenAiEmbeddingResponse> CreateEmbeddingAsync(OpenAiEmbeddingRequest request, CancellationToken cancellationToken = default);
}
