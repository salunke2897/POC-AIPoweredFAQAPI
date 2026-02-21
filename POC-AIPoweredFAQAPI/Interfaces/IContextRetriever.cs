using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Interfaces;

public interface IContextRetriever
{
    Task<IList<FaqItem>> RetrieveAsync(string question, CancellationToken cancellationToken = default);
}
