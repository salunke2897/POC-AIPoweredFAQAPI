using System;

namespace POC_AIPoweredFAQAPI.DTOs
{
    public class EmbeddingCreateResponse
    {
        public Guid Id { get; set; }
        public Guid? SourceId { get; set; }
        public int VectorLength { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
