# featbit-monitor
Monitoring System for FeatBit Service Status

This repository contains a comprehensive monitoring system for FeatBit feature flag service, consisting of four components:

## Components

### 1. .NET Server Monitor (`src/DotNetServerMonitor`)
A long-running .NET console application that connects to FeatBit using the .NET Server SDK. It continuously monitors feature flag values and logs any changes.

**Key Features:**
- Uses FeatBit .NET Server SDK
- Monitors flag values in real-time
- Detects and logs flag value changes
- Configurable check intervals

### 2. JavaScript Client Monitor (`src/JsClientMonitor`)
A long-running Node.js console application that connects to FeatBit using the JavaScript Client SDK. It provides client-side perspective on feature flag behavior.

**Key Features:**
- Uses FeatBit JavaScript Client SDK
- Real-time flag monitoring
- Event-driven flag update notifications
- Configurable polling intervals

### 3. Configuration Changer (`src/ConfigurationChanger`)
A .NET application that programmatically changes FeatBit toggle configurations. Can run as a one-time job or as a recurring cron-like task.

**Key Features:**
- Changes feature flag configurations via FeatBit API
- Supports both one-time and recurring modes
- Configurable change intervals
- API-based flag management

### 4. Validation Application (`src/ValidationApp`)
A .NET application that validates whether the monitors (1) and (2) correctly react to configuration changes made by (3).

**Key Features:**
- Validates monitor responsiveness
- Checks consistency between .NET and JS monitors
- Detects configuration change reactions
- Provides validation summaries and reports

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js 20.x or later
- Access to a FeatBit instance

### Environment Variables

All applications use environment variables for configuration:

```bash
# FeatBit Connection Settings
export FEATBIT_ENV_SECRET="your-env-secret"
export FEATBIT_STREAMING_URI="ws://localhost:5100"
export FEATBIT_EVENT_URI="http://localhost:5100"
export FEATBIT_FLAG_KEY="test-flag"
export FEATBIT_USER_KEY="test-user"
export CHECK_INTERVAL_MS="5000"

# Configuration Changer Settings
export FEATBIT_API_URL="http://localhost:5000"
export FEATBIT_PROJECT_ID="your-project-id"
export FEATBIT_ENV_ID="your-env-id"
export FEATBIT_API_TOKEN="your-api-token"
export CHANGE_MODE="cron"  # or "once"
export CHANGE_INTERVAL_SECONDS="60"

# Validation App Settings
export DOTNET_MONITOR_URL="http://localhost:8001"
export JS_MONITOR_URL="http://localhost:8002"
export VALIDATION_INTERVAL_SECONDS="30"
export CONFIG_CHANGE_WINDOW_SECONDS="15"
```

### Running the Applications

#### 1. Start the .NET Server Monitor
```bash
cd src/DotNetServerMonitor/DotNetServerMonitor
dotnet run
```

#### 2. Start the JavaScript Client Monitor
```bash
cd src/JsClientMonitor
npm start
```

#### 3. Start the Configuration Changer (optional, for testing)
```bash
cd src/ConfigurationChanger/ConfigurationChanger
dotnet run
```

#### 4. Start the Validation Application
```bash
cd src/ValidationApp/ValidationApp
dotnet run
```

## Building the Applications

### Build .NET Applications
```bash
# Build all .NET projects
dotnet build src/DotNetServerMonitor/DotNetServerMonitor/DotNetServerMonitor.csproj
dotnet build src/ConfigurationChanger/ConfigurationChanger/ConfigurationChanger.csproj
dotnet build src/ValidationApp/ValidationApp/ValidationApp.csproj
```

### Install JavaScript Dependencies
```bash
cd src/JsClientMonitor
npm install
```

## Docker Deployment (Future Enhancement)

Docker support can be added for easier deployment. Example Dockerfile structure:

```dockerfile
# .NET Monitor
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY src/DotNetServerMonitor/DotNetServerMonitor/ .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "DotNetServerMonitor.dll"]
```

## Architecture

```
┌─────────────────────┐
│   FeatBit Service   │
└──────────┬──────────┘
           │
     ┌─────┴─────┐
     │           │
┌────▼────┐ ┌───▼─────┐
│  .NET   │ │   JS    │
│ Monitor │ │ Monitor │
└────┬────┘ └───┬─────┘
     │          │
     └────┬─────┘
          │
    ┌─────▼──────┐
    │ Validation │
    │    App     │
    └────────────┘
          ▲
          │
    ┌─────┴──────┐
    │   Config   │
    │  Changer   │
    └────────────┘
```

## Monitoring Workflow

1. **Monitors Start**: Both .NET and JS monitors connect to FeatBit and start polling flag values
2. **Configuration Change**: The Configuration Changer modifies a feature flag
3. **Detection**: Monitors detect the change and log it
4. **Validation**: The Validation App verifies both monitors detected the change correctly
5. **Reporting**: Validation results are logged and summarized

## Use Cases

- **Service Health Monitoring**: Ensure FeatBit service is responding correctly
- **SDK Validation**: Verify both server and client SDKs work as expected
- **Change Detection**: Monitor how quickly flag changes propagate
- **Integration Testing**: Automated testing of feature flag infrastructure
- **Performance Monitoring**: Track response times and flag evaluation performance

## Troubleshooting

### Monitors Not Connecting
- Verify FeatBit service is running
- Check `FEATBIT_STREAMING_URI` and `FEATBIT_EVENT_URI` are correct
- Ensure `FEATBIT_ENV_SECRET` is valid

### Flag Changes Not Detected
- Verify `FEATBIT_FLAG_KEY` exists in your FeatBit project
- Check the flag is enabled and has variations configured
- Ensure monitors have proper permissions

### Configuration Changer Fails
- Verify `FEATBIT_API_TOKEN` has write permissions
- Check `FEATBIT_API_URL` is accessible
- Ensure project and environment IDs are correct

## Contributing

Contributions are welcome! Please follow these guidelines:
1. Create feature branches from `main`
2. Test all components thoroughly
3. Update documentation as needed
4. Submit pull requests with clear descriptions

## License

This project is licensed under the MIT License.

## Support

For issues and questions:
- Create an issue in this repository
- Check FeatBit documentation: https://docs.featbit.co
- Visit FeatBit community forums

