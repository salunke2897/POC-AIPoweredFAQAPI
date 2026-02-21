using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.IRepositories;

public interface IKnowledgeBaseIngestionRepository
{
    Task UpsertAsync(FaqItem item, IList<double> embedding, CancellationToken cancellationToken = default);
}
