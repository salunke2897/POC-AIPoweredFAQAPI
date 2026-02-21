using Microsoft.Data.Sqlite;
using POC_AIPoweredFAQAPI.IRepositories;
using POC_AIPoweredFAQAPI.Models;
using System.Text.Json;

namespace POC_AIPoweredFAQAPI.Repositories;

public class SqliteKnowledgeBaseIngestionRepository : IKnowledgeBaseIngestionRepository
{
    private readonly string _connectionString;

    public SqliteKnowledgeBaseIngestionRepository(Microsoft.Extensions.Options.IOptions<SqliteOptions> options)
    {
        _connectionString = options.Value.ConnectionString ?? "Data Source=faq.db";
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

    public Task UpsertAsync(FaqItem item, IList<double> embedding, CancellationToken cancellationToken = default)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO faq_embeddings(question, answer, embedding) VALUES($q,$a,$e) ON CONFLICT(question) DO UPDATE SET answer=$a, embedding=$e";
        cmd.Parameters.AddWithValue("$q", item.Question);
        cmd.Parameters.AddWithValue("$a", item.Answer);
        cmd.Parameters.AddWithValue("$e", JsonSerializer.Serialize(embedding));
        cmd.ExecuteNonQuery();
        return Task.CompletedTask;
    }
}
