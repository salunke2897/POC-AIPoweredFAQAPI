using System.Text.Json;
using System.Data.Common;
using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Repositories
{
    // Generic Postgres embedding repository using ADO.NET abstractions.
    // At runtime this expects a Postgres provider (Npgsql) to be available; register the provider factory in your host if necessary.
    public class PostgresEmbeddingRepository : IEmbeddingRepository
    {
        private readonly string _connectionString;
        private readonly string _providerInvariantName;

        private void EnsureTable()
        {
            if (string.IsNullOrEmpty(_connectionString)) return;

            var factory = GetFactory();
            using var conn = factory.CreateConnection();
            conn.ConnectionString = _connectionString;
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS embeddings (
                id uuid PRIMARY KEY,
                source_id uuid,
                content text NOT NULL,
                vector jsonb NOT NULL,
                created_at timestamptz NOT NULL DEFAULT now()
            )";
            cmd.ExecuteNonQuery();
        }

        public PostgresEmbeddingRepository(string connectionString, string providerInvariantName = "Npgsql")
        {
            _connectionString = connectionString;
            _providerInvariantName = providerInvariantName;
        EnsureTable();
        }

        private DbProviderFactory GetFactory()
        {
            // This will attempt to resolve a registered provider factory (e.g. Npgsql)
            return DbProviderFactories.GetFactory(_providerInvariantName);
        }

        public async Task AddAsync(EmbeddingRecord record, CancellationToken ct = default)
        {
            var factory = GetFactory();
            await using var conn = factory.CreateConnection();
            conn.ConnectionString = _connectionString;
            await conn.OpenAsync(ct);

            const string sql = @"INSERT INTO embeddings (id, source_id, content, vector, created_at) VALUES (@id, @source_id, @content, @vector::jsonb, @created_at)";

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var pId = cmd.CreateParameter(); pId.ParameterName = "@id"; pId.Value = record.Id; cmd.Parameters.Add(pId);
            var pSource = cmd.CreateParameter(); pSource.ParameterName = "@source_id"; pSource.Value = record.SourceId.HasValue ? (object)record.SourceId.Value : DBNull.Value; cmd.Parameters.Add(pSource);
            var pContent = cmd.CreateParameter(); pContent.ParameterName = "@content"; pContent.Value = record.Content; cmd.Parameters.Add(pContent);
            var vecJson = JsonSerializer.Serialize(record.Vector);
            var pVector = cmd.CreateParameter(); pVector.ParameterName = "@vector"; pVector.Value = vecJson; cmd.Parameters.Add(pVector);
            var pCreated = cmd.CreateParameter(); pCreated.ParameterName = "@created_at"; pCreated.Value = record.CreatedAt; cmd.Parameters.Add(pCreated);

            if (conn is DbConnection dbConn)
            {
                await cmd.ExecuteNonQueryAsync(ct);
            }
        }

        public async Task<IEnumerable<EmbeddingRecord>> GetAllAsync(CancellationToken ct = default)
        {
            var results = new List<EmbeddingRecord>();
            var factory = GetFactory();
            await using var conn = factory.CreateConnection();
            conn.ConnectionString = _connectionString;
            await conn.OpenAsync(ct);

            const string sql = @"SELECT id, source_id, content, vector, created_at FROM embeddings";
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = reader.GetGuid(0);
                var source = await reader.IsDBNullAsync(1, ct) ? (Guid?)null : reader.GetGuid(1);
                var content = await reader.IsDBNullAsync(2, ct) ? string.Empty : reader.GetString(2);
                var vectorJson = await reader.IsDBNullAsync(3, ct) ? "[]" : reader.GetString(3);
                var createdAt = await reader.IsDBNullAsync(4, ct) ? DateTime.UtcNow : reader.GetDateTime(4);

                float[] vector;
                try
                {
                    vector = JsonSerializer.Deserialize<float[]>(vectorJson) ?? Array.Empty<float>();
                }
                catch
                {
                    vector = Array.Empty<float>();
                }

                results.Add(new EmbeddingRecord
                {
                    Id = id,
                    SourceId = source,
                    Content = content,
                    Vector = vector,
                    CreatedAt = createdAt
                });
            }

            return results;
        }
    }
}
