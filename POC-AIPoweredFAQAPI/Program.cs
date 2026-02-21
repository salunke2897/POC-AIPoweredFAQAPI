using Microsoft.Extensions.Options;
using Polly;
using POC_AIPoweredFAQAPI.Infrastructure;
using POC_AIPoweredFAQAPI.Interfaces;
using POC_AIPoweredFAQAPI.IRepositories;
using POC_AIPoweredFAQAPI.Repositories;
using Npgsql;
using System.Data.Common;
using POC_AIPoweredFAQAPI.Services;
using POC_AIPoweredFAQAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<PostgresOptions>(builder.Configuration.GetSection("Postgres"));
builder.Services.Configure<SqliteOptions>(builder.Configuration.GetSection("Sqlite"));
builder.Services.Configure<RagOptions>(builder.Configuration.GetSection("Rag"));

// Interfaces (Services)
builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<IFaqIngestionService, FaqIngestionService>();
builder.Services.AddScoped<IPromptBuilder, PromptBuilder>();
builder.Services.AddScoped<IContextRetriever, ContextRetriever>();

// Repositories interfaces
builder.Services.AddSingleton<IConversationRepository, InMemoryConversationRepository>();
builder.Services.AddSingleton<IFaqRepository, InMemoryFaqRepository>();

var ragOptions = builder.Configuration.GetSection("Rag").Get<RagOptions>() ?? new RagOptions { Provider = "Sqlite", MaxResults = 5 };

if (string.Equals(ragOptions.Provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IKnowledgeBaseRepository, SqliteKnowledgeBaseRepository>();
    builder.Services.AddSingleton<IKnowledgeBaseIngestionRepository, SqliteKnowledgeBaseIngestionRepository>();
}
else
{
    // Postgres
    builder.Services.AddSingleton<IKnowledgeBaseRepository, PostgresKnowledgeBaseRepository>();
    builder.Services.AddSingleton<IKnowledgeBaseIngestionRepository, PostgresKnowledgeBaseIngestionRepository>();

    // Ensure Npgsql provider factory is registered for ADO.NET provider scenarios
    try
    {
        // Registering the factory allows generic provider-based repositories to resolve Npgsql
        DbProviderFactories.RegisterFactory("Npgsql", NpgsqlFactory.Instance);
    }
    catch
    {
        // Ignore registration errors (factory may already be registered)
    }

    var pgOptions = builder.Configuration.GetSection("Postgres").Get<PostgresOptions>();
    if (!string.IsNullOrEmpty(pgOptions?.ConnectionString))
    {
        // Register embedding repository implementation for Postgres
        builder.Services.AddSingleton<IEmbeddingRepository>(sp =>
        {
            return new PostgresEmbeddingRepository(pgOptions.ConnectionString, "Npgsql");
        });
    }
}

// HttpClients with Polly retry
var retryPolicy = RetryPolicyProvider.GetRetryPolicy();

builder.Services.AddHttpClient<IAiClient, OpenAiClient>()
    .AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient<IEmbeddingClient, OpenAiEmbeddingClient>()
    .AddPolicyHandler(retryPolicy);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
