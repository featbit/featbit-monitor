using System.Text;
using System.Text.Json;

Console.WriteLine("FeatBit Configuration Changer Starting...");

// Read configuration from environment variables
var apiUrl = Environment.GetEnvironmentVariable("FEATBIT_API_URL") ?? "http://localhost:5000";
var projectId = Environment.GetEnvironmentVariable("FEATBIT_PROJECT_ID") ?? "your-project-id";
var envId = Environment.GetEnvironmentVariable("FEATBIT_ENV_ID") ?? "your-env-id";
var apiToken = Environment.GetEnvironmentVariable("FEATBIT_API_TOKEN") ?? "your-api-token";
var flagKey = Environment.GetEnvironmentVariable("FEATBIT_FLAG_KEY") ?? "test-flag";
var mode = Environment.GetEnvironmentVariable("CHANGE_MODE") ?? "cron"; // "cron" or "once"
var intervalSeconds = int.Parse(Environment.GetEnvironmentVariable("CHANGE_INTERVAL_SECONDS") ?? "60");

Console.WriteLine($"Configuration:");
Console.WriteLine($"  API URL: {apiUrl}");
Console.WriteLine($"  Project ID: {projectId}");
Console.WriteLine($"  Environment ID: {envId}");
Console.WriteLine($"  Flag Key: {flagKey}");
Console.WriteLine($"  Mode: {mode}");
Console.WriteLine($"  Interval: {intervalSeconds} seconds");

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("Authorization", apiToken);

async Task<bool> ToggleFlagAsync()
{
    try
    {
        // Get current flag configuration
        var getUrl = $"{apiUrl}/api/v1/envs/{envId}/feature-flags/{flagKey}";
        Console.WriteLine($"\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Fetching flag configuration...");
        
        var getResponse = await httpClient.GetAsync(getUrl);
        if (!getResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error: Failed to get flag configuration. Status: {getResponse.StatusCode}");
            return false;
        }

        var flagData = await getResponse.Content.ReadAsStringAsync();
        var flagJson = JsonDocument.Parse(flagData);
        
        // Toggle the flag (simplified - just changing a variation value or toggling on/off)
        // In a real scenario, you might want to modify specific properties
        var updateUrl = $"{apiUrl}/api/v1/envs/{envId}/feature-flags/{flagKey}";
        
        // Create a simple toggle payload
        // Note: This is a simplified example. Real implementation would depend on FeatBit API structure
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var updatePayload = new
        {
            comment = $"Auto-toggle at {timestamp}",
            // Add other necessary fields based on FeatBit API requirements
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(updatePayload),
            Encoding.UTF8,
            "application/json");

        var updateResponse = await httpClient.PatchAsync(updateUrl, jsonContent);
        
        if (updateResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Successfully toggled flag '{flagKey}'");
            return true;
        }
        else
        {
            Console.WriteLine($"Error: Failed to update flag. Status: {updateResponse.StatusCode}");
            var errorContent = await updateResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {errorContent}");
            return false;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error toggling flag: {ex.Message}");
        return false;
    }
}

// Main execution loop
if (mode.ToLower() == "once")
{
    Console.WriteLine("\nRunning in ONCE mode - will change configuration once and exit.");
    var success = await ToggleFlagAsync();
    Environment.Exit(success ? 0 : 1);
}
else
{
    Console.WriteLine($"\nRunning in CRON mode - will change configuration every {intervalSeconds} seconds.");
    Console.WriteLine("Press Ctrl+C to exit.\n");
    
    while (true)
    {
        await ToggleFlagAsync();
        await Task.Delay(TimeSpan.FromSeconds(intervalSeconds));
    }
}
