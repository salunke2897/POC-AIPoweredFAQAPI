using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Interfaces;

public interface IFaqService
{
    Task<FaqAskResponse> AskAsync(FaqAskRequest request, CancellationToken cancellationToken = default);
}
