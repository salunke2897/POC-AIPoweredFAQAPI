using System;

namespace POC_AIPoweredFAQAPI.DTOs
{
    public class EmbeddingCreateRequest
    {
        public string Content { get; set; } = string.Empty;
        public Guid? SourceId { get; set; }
    }
}
