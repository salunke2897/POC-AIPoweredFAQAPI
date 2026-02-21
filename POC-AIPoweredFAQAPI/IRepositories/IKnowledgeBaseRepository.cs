using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.IRepositories;

public interface IKnowledgeBaseRepository
{
    Task<IList<FaqItem>> QueryByEmbeddingAsync(IList<double> embedding, int limit, CancellationToken cancellationToken = default);
}
