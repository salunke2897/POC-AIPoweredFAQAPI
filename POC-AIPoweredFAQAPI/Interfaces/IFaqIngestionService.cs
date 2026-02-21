using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Interfaces;

public interface IFaqIngestionService
{
    Task IngestAsync(FaqIngestRequest request, CancellationToken cancellationToken = default);
}
