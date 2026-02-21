using POC_AIPoweredFAQAPI.IRepositories;
using POC_AIPoweredFAQAPI.Models;
using System.Collections.Concurrent;

namespace POC_AIPoweredFAQAPI.Repositories;

public class InMemoryConversationRepository : IConversationRepository
{
    private readonly ConcurrentBag<Conversation> _store = new();

    public Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        _store.Add(conversation);
        return Task.CompletedTask;
    }

    public Task<IList<Conversation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IList<Conversation>)_store.ToList());
    }
}
