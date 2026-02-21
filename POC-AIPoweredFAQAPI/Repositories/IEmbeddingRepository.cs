using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Repositories
{
    public interface IEmbeddingRepository
    {
        Task AddAsync(EmbeddingRecord record, CancellationToken ct = default);
        Task<IEnumerable<EmbeddingRecord>> GetAllAsync(CancellationToken ct = default);
    }
}
