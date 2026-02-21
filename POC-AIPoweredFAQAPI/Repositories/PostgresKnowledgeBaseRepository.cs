using Npgsql;
using POC_AIPoweredFAQAPI.IRepositories;
using POC_AIPoweredFAQAPI.Models;
using System.Globalization;
using System.Text;
using System.Linq;

namespace POC_AIPoweredFAQAPI.Repositories;

public class PostgresKnowledgeBaseRepository : IKnowledgeBaseRepository
{
    private readonly string _connectionString;
    private readonly int _maxResults;
    private bool _usePgVector = true;

    public PostgresKnowledgeBaseRepository(Microsoft.Extensions.Options.IOptions<PostgresOptions> options, Microsoft.Extensions.Options.IOptions<RagOptions> ragOptions)
    {
        _connectionString = options.Value.ConnectionString ?? string.Empty;
        _maxResults = ragOptions.Value.MaxResults;
        EnsureTable();
    }

    private void EnsureTable()
    {
        if (string.IsNullOrEmpty(_connectionString)) return;

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        // Try to create pgvector extension and table with vector column. If pgvector is not available,
        // fall back to using a double precision[] column for embeddings.
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
            // pgvector extension not available; use double precision[] fallback
            _usePgVector = false;
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS faq_embeddings (
                                    question TEXT PRIMARY KEY,
                                    answer TEXT,
                                    embedding double precision[]
                                 );";
            cmd.ExecuteNonQuery();

            // Minimal informative output for developer environments
            Console.WriteLine("pgvector extension not available on server; using double precision[] fallback for embeddings.");
        }
    }

    public async Task<IList<FaqItem>> QueryByEmbeddingAsync(IList<double> embedding, int limit, CancellationToken cancellationToken = default)
    {
        var result = new List<FaqItem>();
        if (string.IsNullOrEmpty(_connectionString)) return result;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        // Convert embedding to Postgres vector literal
        var sb = new StringBuilder("[");
        for (int i = 0; i < embedding.Count; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(embedding[i].ToString(CultureInfo.InvariantCulture));
        }
        sb.Append(']');
        var vectorLiteral = sb.ToString();
        if (_usePgVector)
        {
            var sql = "SELECT question, answer FROM faq_embeddings ORDER BY embedding <-> @embedding::vector LIMIT @limit";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@embedding", vectorLiteral);
            cmd.Parameters.AddWithValue("@limit", limit);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var q = reader.GetString(0);
                var a = reader.GetString(1);
                result.Add(new FaqItem { Question = q, Answer = a });
            }
        }
        else
        {
            // Fall back: read stored double precision[] embeddings and compute distances in C#
            var sql = "SELECT question, answer, embedding FROM faq_embeddings";
            await using var cmd = new NpgsqlCommand(sql, conn);
            var rows = new List<(string Question, string Answer, double Distance)>();

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var q = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                var a = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);

                double distance = double.MaxValue;

                if (!await reader.IsDBNullAsync(2, cancellationToken))
                {
                    try
                    {
                        // Npgsql maps PostgreSQL arrays to .NET arrays
                        var stored = reader.GetFieldValue<double[]>(2);
                        if (stored != null && stored.Length == embedding.Count)
                        {
                            double sum = 0;
                            for (int i = 0; i < stored.Length; i++)
                            {
                                var d = stored[i] - embedding[i];
                                sum += d * d;
                            }
                            distance = Math.Sqrt(sum);
                        }
                    }
                    catch
                    {
                        distance = double.MaxValue;
                    }
                }

                rows.Add((q, a, distance));
            }

            foreach (var r in rows.OrderBy(x => x.Distance).Take(limit))
            {
                result.Add(new FaqItem { Question = r.Question, Answer = r.Answer });
            }
        }

        return result;
    }
}
