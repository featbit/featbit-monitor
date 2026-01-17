# FeatBit Feature Flag Monitor

A .NET 10 Azure Function app that monitors and toggles FeatBit feature flags, then verifies the changes.

## Features

- Toggles a FeatBit feature flag via PUT API request
- Verifies the flag status via GET API request
- Runs on a timer schedule (default: every 5 minutes)
- Configurable through environment variables
- Supports Azure Functions deployment

## Prerequisites

- .NET 10 SDK
- Azure Functions Core Tools v4
- FeatBit account with personal access token

## Configuration

Update the `local.settings.json` file with your values:

```json
{
  "Values": {
    "FeatBit_EnvId": "your-environment-id",
    "FeatBit_FeatureFlagKey": "your-feature-flag-key",
    "FeatBit_PersonalToken": "your-personal-token",
    "FeatBit_TargetStatus": "true",
    "Verification_BaseUrl": "https://localhost:54277",
    "Verification_Type": "default"
  }
}
```

### Configuration Values

- `FeatBit_EnvId`: Your FeatBit environment ID
- `FeatBit_FeatureFlagKey`: The feature flag key to toggle
- `FeatBit_PersonalToken`: Your FeatBit personal access token
- `FeatBit_TargetStatus`: Target status for the flag (true/false)
- `Verification_BaseUrl`: Base URL for verification API
- `Verification_Type`: Type parameter for verification endpoint

## Local Development

1. Install dependencies:
```bash
dotnet restore
```

2. Run the function locally:
```bash
func start
```

Or use:
```bash
dotnet run
```

## How It Works

1. **Timer Trigger**: The function runs every 5 minutes (configurable via cron expression)
2. **Toggle Flag**: Makes a PUT request to FeatBit API to toggle the feature flag
3. **Wait**: Waits 2 seconds for changes to propagate
4. **Verify**: Makes a GET request to verification endpoint to check the status
5. **Compare**: Compares the returned value with the expected status

## Timer Schedule

The default schedule is `0 */5 * * * *` (every 5 minutes). Modify the `TimerTrigger` attribute in `FeatureFlagMonitor.cs` to change this:

```csharp
[Function("FeatureFlagMonitor")]
public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
```

Common schedules:
- `0 */5 * * * *` - Every 5 minutes
- `0 0 * * * *` - Every hour
- `0 0 */6 * * *` - Every 6 hours
- `0 30 9 * * *` - Daily at 9:30 AM

## Deployment to Azure

1. Create an Azure Function App (Windows or Linux, .NET 10)

2. Deploy using Azure Functions Core Tools:
```bash
func azure functionapp publish <your-function-app-name>
```

3. Configure application settings in Azure Portal:
   - Navigate to your Function App
   - Go to Configuration > Application Settings
   - Add all the environment variables from `local.settings.json`

## API Endpoints

### FeatBit Toggle API
```
PUT https://app-api.featbit.co/api/v1/envs/{envId}/feature-flags/{feature-flag-key}/toggle/{status}
Authorization: Bearer {personal-token}
```

### Verification API
```
GET {baseUrl}/api/Features/{feature-flag-key}?type={type}
```

## Logging

The function uses `ILogger` for logging. View logs:
- Locally: Check console output
- Azure: Use Application Insights or Log Stream in Azure Portal

## Error Handling

The function includes error handling for:
- Missing configuration
- HTTP request failures
- JSON parsing errors
- Certificate validation (for localhost development)

## Notes

- The function allows self-signed certificates for localhost development
- Multiple JSON response formats are supported for verification
- A 2-second delay is included after toggling to allow propagation
