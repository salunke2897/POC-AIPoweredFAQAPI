using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using POC_AIPoweredFAQAPI.Interfaces;
using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Services;

public class OpenAiEmbeddingClient : IEmbeddingClient
{
    private readonly HttpClient _http;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiEmbeddingClient> _logger;

    public OpenAiEmbeddingClient(HttpClient http, Microsoft.Extensions.Options.IOptions<OpenAiOptions> options, ILogger<OpenAiEmbeddingClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }
    }

    public async Task<OpenAiEmbeddingResponse> CreateEmbeddingAsync(OpenAiEmbeddingRequest request, CancellationToken cancellationToken = default)
    {
        var endpoint = _options.EmbeddingsEndpoint ?? string.Empty;
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (request.Input == null || request.Input.Count == 0) throw new ArgumentException("Embedding request must contain at least one input.", nameof(request));

        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var payload = JsonSerializer.Serialize(request, jsonOptions);
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _http.PostAsync(endpoint, content, cancellationToken);
            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Embeddings endpoint returned {StatusCode}: {Body}", (int)res.StatusCode, err);
                throw new HttpRequestException($"Embeddings endpoint returned {(int)res.StatusCode}: {err}");
            }

            var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
            var resp = await JsonSerializer.DeserializeAsync<OpenAiEmbeddingResponse>(stream, jsonOptions, cancellationToken) ?? new OpenAiEmbeddingResponse();
            return resp;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when calling embeddings endpoint {Endpoint}", endpoint);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse embedding response from {Endpoint}", endpoint);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateEmbeddingAsync");
            throw;
        }
    }
}
