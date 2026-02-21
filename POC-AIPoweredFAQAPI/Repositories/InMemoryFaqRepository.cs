using POC_AIPoweredFAQAPI.IRepositories;
using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Repositories;

public class InMemoryFaqRepository : IFaqRepository
{
    private static readonly List<FaqItem> _items = new()
    {
        new FaqItem { Question = "What is the API for?", Answer = "Answer: FAQ API." }
    };

    public Task AddAsync(FaqItem item, CancellationToken cancellationToken = default)
    {
        _items.Add(item);
        return Task.CompletedTask;
    }

    public Task<IList<FaqItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IList<FaqItem>)_items.ToList());
    }
}
