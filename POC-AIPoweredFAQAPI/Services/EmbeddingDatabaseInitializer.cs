using System.Data.Common;
using System.Text;
using POC_AIPoweredFAQAPI.Repositories;

namespace POC_AIPoweredFAQAPI.Services
{
    public class EmbeddingDatabaseInitializer
    {
        private readonly string _connectionString;
        private readonly string _providerInvariantName;

        public EmbeddingDatabaseInitializer(string connectionString, string providerInvariantName = "Npgsql")
        {
            _connectionString = connectionString;
            _providerInvariantName = providerInvariantName;
        }

        private DbProviderFactory GetFactory()
        {
            return DbProviderFactories.GetFactory(_providerInvariantName);
        }

        public async Task EnsureCreatedAsync(CancellationToken ct = default)
        {
            var factory = GetFactory();
            await using var conn = factory.CreateConnection();
            conn.ConnectionString = _connectionString;
            await conn.OpenAsync(ct);

            // Create embeddings table if it does not exist. Uses jsonb for vector storage.
            var sql = new StringBuilder();
            sql.AppendLine("CREATE TABLE IF NOT EXISTS embeddings (");
            sql.AppendLine("  id uuid PRIMARY KEY,");
            sql.AppendLine("  source_id uuid NULL,");
            sql.AppendLine("  content text NOT NULL,");
            sql.AppendLine("  vector jsonb NOT NULL,");
            sql.AppendLine("  created_at timestamptz NOT NULL DEFAULT now()");
            sql.AppendLine(");");

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql.ToString();
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
