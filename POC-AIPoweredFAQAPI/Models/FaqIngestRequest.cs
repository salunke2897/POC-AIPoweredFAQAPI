using System.Collections.ObjectModel;

namespace POC_AIPoweredFAQAPI.Models;

public class FaqIngestRequest
{
    public IReadOnlyList<FaqItem> Items { get; set; } = Array.Empty<FaqItem>();
}
