using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FlagChangeTrigger;

public class FeatureFlagMonitor
{
    private readonly ILogger<FeatureFlagMonitor> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public FeatureFlagMonitor(ILogger<FeatureFlagMonitor> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [Function("FeatureFlagMonitor")]
    public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("Feature Flag Monitor function executed at: {time}", DateTime.UtcNow);

        // Get configuration from environment variables
        var envId = Environment.GetEnvironmentVariable("FeatBit_EnvId");
        var featureFlagKey = Environment.GetEnvironmentVariable("FeatBit_FeatureFlagKey");
        var personalToken = Environment.GetEnvironmentVariable("FeatBit_PersonalToken");
        var targetStatus = Environment.GetEnvironmentVariable("FeatBit_TargetStatus") ?? "true";
        var verificationUrl = Environment.GetEnvironmentVariable("Verification_BaseUrl") ?? "https://localhost:54277";
        var verificationType = Environment.GetEnvironmentVariable("Verification_Type") ?? "default";

        if (string.IsNullOrEmpty(envId) || string.IsNullOrEmpty(featureFlagKey) || string.IsNullOrEmpty(personalToken))
        {
            _logger.LogError("Missing required configuration: FeatBit_EnvId, FeatBit_FeatureFlagKey, or FeatBit_PersonalToken");
            return;
        }

        try
        {
            // Step 1: Toggle the feature flag
            var toggleResult = await ToggleFeatureFlag(envId, featureFlagKey, targetStatus, personalToken);
            
            if (!toggleResult)
            {
                _logger.LogError("Failed to toggle feature flag");
                return;
            }

            _logger.LogInformation("Successfully toggled feature flag to status: {status}", targetStatus);

            // Wait a moment for the change to propagate
            await Task.Delay(TimeSpan.FromSeconds(2));

            // Step 2: Verify the feature flag status
            var verificationResult = await VerifyFeatureFlagStatus(verificationUrl, featureFlagKey, verificationType, targetStatus);

            if (verificationResult)
            {
                _logger.LogInformation("Feature flag status verified successfully. Status matches: {status}", targetStatus);
            }
            else
            {
                _logger.LogWarning("Feature flag status verification failed. Expected: {expected}, but verification did not match", targetStatus);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during feature flag monitoring");
        }

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {next}", myTimer.ScheduleStatus.Next);
        }
    }

    private async Task<bool> ToggleFeatureFlag(string envId, string featureFlagKey, string status, string personalToken)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"https://app-api.featbit.co/api/v1/envs/{envId}/feature-flags/{featureFlagKey}/toggle/{status}";

        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", personalToken);

        try
        {
            var response = await client.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully called FeatBit API: {statusCode}", response.StatusCode);
                return true;
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogError("FeatBit API returned error: {statusCode}, Content: {content}", response.StatusCode, content);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while calling FeatBit API");
            return false;
        }
    }

    private async Task<bool> VerifyFeatureFlagStatus(string baseUrl, string featureFlagKey, string type, string expectedStatus)
    {
        var client = _httpClientFactory.CreateClient();
        
        // Configure to allow self-signed certificates for localhost development
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        var localClient = new HttpClient(handler);

        var url = $"{baseUrl}/api/Features/{featureFlagKey}?type={type}";

        try
        {
            var response = await localClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Verification API response: {content}", content);

                // Try to parse as JSON first
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    
                    // Check various possible response formats
                    if (doc.RootElement.TryGetProperty("status", out var statusProp))
                    {
                        var actualStatus = statusProp.GetString();
                        return string.Equals(actualStatus, expectedStatus, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (doc.RootElement.TryGetProperty("enabled", out var enabledProp))
                    {
                        var actualStatus = enabledProp.GetBoolean().ToString().ToLower();
                        return string.Equals(actualStatus, expectedStatus, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.True || doc.RootElement.ValueKind == JsonValueKind.False)
                    {
                        var actualStatus = doc.RootElement.GetBoolean().ToString().ToLower();
                        return string.Equals(actualStatus, expectedStatus, StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch (JsonException)
                {
                    // If not JSON, compare as plain text
                    return string.Equals(content.Trim(), expectedStatus, StringComparison.OrdinalIgnoreCase);
                }

                _logger.LogWarning("Could not find status field in response: {content}", content);
                return false;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Verification API returned error: {statusCode}, Content: {content}", response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while calling verification API");
            return false;
        }
        finally
        {
            localClient.Dispose();
        }
    }
}
