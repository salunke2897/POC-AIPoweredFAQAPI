using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.IRepositories;

public interface IFaqRepository
{
    Task<IList<FaqItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(FaqItem item, CancellationToken cancellationToken = default);
}
