using Polly;
using Polly.Retry;
using System.Net;

namespace POC_AIPoweredFAQAPI.Infrastructure;

public static class RetryPolicyProvider
{
    public static AsyncRetryPolicy<System.Net.Http.HttpResponseMessage> GetRetryPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => (int)msg.StatusCode == 429 || (int)msg.StatusCode >= 500)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
