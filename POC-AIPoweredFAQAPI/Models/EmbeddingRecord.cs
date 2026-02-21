namespace POC_AIPoweredFAQAPI.Models
{
    public class EmbeddingRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? SourceId { get; set; }
        public string Content { get; set; } = string.Empty;
        public float[] Vector { get; set; } = Array.Empty<float>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
