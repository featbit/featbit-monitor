using FeatBit.Sdk.Server;
using FeatBit.Sdk.Server.Options;
using FeatBit.Sdk.Server.Model;

Console.WriteLine("FeatBit .NET Server Monitor Starting...");

// Read configuration from environment variables
var envSecret = Environment.GetEnvironmentVariable("FEATBIT_ENV_SECRET") ?? "your-env-secret";
var flagKey = Environment.GetEnvironmentVariable("FEATBIT_FLAG_KEY") ?? "test-flag";

// Generate unique user key with UTC timestamp
var utcTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
var userKey = $"featbit_monitor_user_dotnet_{utcTimestamp}";

// Use FeatBit Cloud URIs
const string streamingUri = "wss://app-eval.featbit.co";
const string eventUri = "https://app-eval.featbit.co";

Console.WriteLine($"Configuration:");
Console.WriteLine($"  Streaming URI: {streamingUri}");
Console.WriteLine($"  Event URI: {eventUri}");
Console.WriteLine($"  Flag Key: {flagKey}");
Console.WriteLine($"  User Key: {userKey}");

// Initialize FeatBit client with proper configuration
var options = new FbOptionsBuilder(envSecret)
    .Streaming(new Uri(streamingUri))
    .Event(new Uri(eventUri))
    .StartWaitTime(TimeSpan.FromSeconds(15))
    .DisableEvents(false) 
    .Build();

var client = new FbClient(options);

// Wait for initialization
var maxWaitTime = TimeSpan.FromSeconds(30);
var startTime = DateTime.UtcNow;
while (!client.Initialized && (DateTime.UtcNow - startTime) < maxWaitTime)
{
    await Task.Delay(100);
}

if (client.Initialized)
{
    Console.WriteLine("FeatBit client initialized successfully.");
}
else
{
    Console.WriteLine("Warning: FeatBit client initialization timed out, but will continue.");
}

// Create a user for evaluation
var user = FbUser.Builder(userKey).Build();

// Monitor the flag in a loop
var lastValue = string.Empty;
var checkInterval = int.Parse(Environment.GetEnvironmentVariable("CHECK_INTERVAL_MS") ?? "5000");

Console.WriteLine($"\nMonitoring flag '{flagKey}' every {checkInterval}ms...");
Console.WriteLine("Press Ctrl+C to exit.\n");

// Handle graceful shutdown
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("\nShutting down gracefully...");
};

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        try
        {
            // Get flag variation
            var variation = client.StringVariation(flagKey, user, "default-value");
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            
            if (variation != lastValue)
            {
                Console.WriteLine($"[{timestamp}] FLAG CHANGED: '{flagKey}' = '{variation}' (previous: '{lastValue}')");
                lastValue = variation;
            }
            else
            {
                Console.WriteLine($"[{timestamp}] Flag: '{flagKey}' = '{variation}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error evaluating flag: {ex.Message}");
        }

        await Task.Delay(checkInterval, cts.Token);
    }
}
catch (TaskCanceledException)
{
    // Expected when cancellation is requested
}

// Close client properly to ensure any pending events are sent
await client.CloseAsync();
Console.WriteLine("FeatBit client closed. Exiting...");
