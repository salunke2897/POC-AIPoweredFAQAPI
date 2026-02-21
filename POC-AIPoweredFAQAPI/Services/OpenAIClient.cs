using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using POC_AIPoweredFAQAPI.Interfaces;
using POC_AIPoweredFAQAPI.Models;

namespace POC_AIPoweredFAQAPI.Services;

public class OpenAiClient : IAiClient
{
    private readonly HttpClient _http;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiClient> _logger;

    public OpenAiClient(HttpClient http, Microsoft.Extensions.Options.IOptions<OpenAiOptions> options, ILogger<OpenAiClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }
    }

    public async Task<OpenAiChatResponse> SendChatAsync(OpenAiChatRequest request, CancellationToken cancellationToken = default)
    {
        var endpoint = _options.Endpoint ?? string.Empty;
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (request.Messages == null || request.Messages.Count == 0) throw new ArgumentException("Chat request must contain messages.", nameof(request));

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
                var body = await res.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Chat completions endpoint returned {StatusCode}: {Body}", (int)res.StatusCode, body);
                throw new HttpRequestException($"Chat completions endpoint returned {(int)res.StatusCode}: {body}");
            }

            var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
            var resp = await JsonSerializer.DeserializeAsync<OpenAiChatResponse>(stream, jsonOptions, cancellationToken) ?? new OpenAiChatResponse();
            return resp;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when calling chat completions endpoint {Endpoint}", endpoint);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse chat response from {Endpoint}", endpoint);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SendChatAsync");
            throw;
        }
    }
}
