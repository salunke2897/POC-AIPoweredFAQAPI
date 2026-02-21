using Microsoft.Data.Sqlite;
using POC_AIPoweredFAQAPI.IRepositories;
using POC_AIPoweredFAQAPI.Models;
using System.Text.Json;

namespace POC_AIPoweredFAQAPI.Repositories;

public class SqliteKnowledgeBaseRepository : IKnowledgeBaseRepository
{
    private readonly string _connectionString;
    private readonly int _maxResults;

    public SqliteKnowledgeBaseRepository(Microsoft.Extensions.Options.IOptions<SqliteOptions> options, Microsoft.Extensions.Options.IOptions<RagOptions> ragOptions)
    {
        _connectionString = options.Value.ConnectionString ?? "Data Source=faq.db";
        _maxResults = ragOptions.Value.MaxResults;
        EnsureTable();
    }

    private void EnsureTable()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS faq_embeddings (question TEXT PRIMARY KEY, answer TEXT, embedding TEXT)";
        cmd.ExecuteNonQuery();
    }

    public Task<IList<FaqItem>> QueryByEmbeddingAsync(IList<double> embedding, int limit, CancellationToken cancellationToken = default)
    {
        var results = new List<(FaqItem item, double score)>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT question, answer, embedding FROM faq_embeddings";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var q = reader.GetString(0);
            var a = reader.GetString(1);
            var embJson = reader.GetString(2);
            try
            {
                var stored = JsonSerializer.Deserialize<List<double>>(embJson) ?? new List<double>();
                var score = CosineSimilarity(embedding, stored);
                results.Add((new FaqItem { Question = q, Answer = a }, score));
            }
            catch { }
        }

        var top = results.OrderByDescending(r => r.score).Take(limit).Select(r => r.item).ToList();
        return Task.FromResult((IList<FaqItem>)top);
    }

    private static double CosineSimilarity(IList<double> a, IList<double> b)
    {
        if (a.Count != b.Count) return 0;
        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Count; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }
        if (na == 0 || nb == 0) return 0;
        return dot / (Math.Sqrt(na) * Math.Sqrt(nb));
    }
}
