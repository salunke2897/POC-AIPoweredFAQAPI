using Npgsql;
using POC_AIPoweredFAQAPI.IRepositories;
using POC_AIPoweredFAQAPI.Models;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace POC_AIPoweredFAQAPI.Repositories;

public class PostgresKnowledgeBaseIngestionRepository : IKnowledgeBaseIngestionRepository
{
    private readonly string _connectionString;
    private bool _usePgVector = true;

    public PostgresKnowledgeBaseIngestionRepository(Microsoft.Extensions.Options.IOptions<PostgresOptions> options)
    {
        _connectionString = options.Value.ConnectionString ?? string.Empty;
        EnsureTable();
    }

    private void EnsureTable()
    {
        if (string.IsNullOrEmpty(_connectionString)) return;

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        // try to create pgvector extension and table with vector column
        try
        {
            cmd.CommandText = "CREATE EXTENSION IF NOT EXISTS vector;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS faq_embeddings (
                                    question TEXT PRIMARY KEY,
                                    answer TEXT,
                                    embedding vector(1536)
                                 );";
            cmd.ExecuteNonQuery();
            _usePgVector = true;
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "0A000")
        {
            // pgvector extension is not available on this server/environment.
            // Fall back to using a double precision array for embeddings.
            _usePgVector = false;

            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS faq_embeddings (
                                    question TEXT PRIMARY KEY,
                                    answer TEXT,
                                    embedding double precision[]
                                 );";
            cmd.ExecuteNonQuery();

            // Informative output for developer; keep minimal (no DI logger here)
            Console.WriteLine("pgvector extension not available on server; using double precision[] fallback for embeddings.");
        }
    }

    public async Task UpsertAsync(FaqItem item, IList<double> embedding, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_connectionString)) return;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        if (_usePgVector)
        {
            // Convert embedding to Postgres vector literal
            var sb = new StringBuilder("[");
            for (int i = 0; i < embedding.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(embedding[i].ToString(CultureInfo.InvariantCulture));
            }
            sb.Append(']');
            var vectorLiteral = sb.ToString();

            // Use parameterized upsert, casting the literal to vector
            var sql = @"INSERT INTO faq_embeddings(question, answer, embedding) VALUES(@q,@a,@e::vector)
ON CONFLICT (question) DO UPDATE SET answer = EXCLUDED.answer, embedding = EXCLUDED.embedding";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@q", item.Question);
            cmd.Parameters.AddWithValue("@a", item.Answer);
            cmd.Parameters.AddWithValue("@e", vectorLiteral);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        else
        {
            // Fallback: store embedding as double precision[]
            var sql = @"INSERT INTO faq_embeddings(question, answer, embedding) VALUES(@q,@a,@e)
ON CONFLICT (question) DO UPDATE SET answer = EXCLUDED.answer, embedding = EXCLUDED.embedding";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@q", item.Question);
            cmd.Parameters.AddWithValue("@a", item.Answer);
            // pass as array of double
            cmd.Parameters.AddWithValue("@e", embedding.ToArray());
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
