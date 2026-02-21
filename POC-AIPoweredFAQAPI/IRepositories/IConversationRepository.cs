using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.IRepositories;

public interface IConversationRepository
{
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task<IList<Conversation>> GetAllAsync(CancellationToken cancellationToken = default);
}
