using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Interfaces;

public interface IAiClient
{
    Task<OpenAiChatResponse> SendChatAsync(OpenAiChatRequest request, CancellationToken cancellationToken = default);
}
