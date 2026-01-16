using FeatBit.Sdk.Server;
using FeatBit.Sdk.Server.Options;
using FeatBit.Sdk.Server.Model;

Console.WriteLine("FeatBit .NET Server Monitor Starting...");

// Read configuration from environment variables
var envSecret = Environment.GetEnvironmentVariable("FEATBIT_ENV_SECRET") ?? "your-env-secret";
var streamingUri = Environment.GetEnvironmentVariable("FEATBIT_STREAMING_URI") ?? "ws://localhost:5100";
var eventUri = Environment.GetEnvironmentVariable("FEATBIT_EVENT_URI") ?? "http://localhost:5100";
var flagKey = Environment.GetEnvironmentVariable("FEATBIT_FLAG_KEY") ?? "test-flag";
var userKey = Environment.GetEnvironmentVariable("FEATBIT_USER_KEY") ?? "test-user";

Console.WriteLine($"Configuration:");
Console.WriteLine($"  Streaming URI: {streamingUri}");
Console.WriteLine($"  Event URI: {eventUri}");
Console.WriteLine($"  Flag Key: {flagKey}");
Console.WriteLine($"  User Key: {userKey}");

// Initialize FeatBit client
var options = new FbOptionsBuilder(envSecret)
    .Streaming(new Uri(streamingUri))
    .Event(new Uri(eventUri))
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

while (true)
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

    await Task.Delay(checkInterval);
}
