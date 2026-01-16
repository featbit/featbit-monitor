# .NET Server Monitor

Long-running .NET application that monitors FeatBit feature flags using the .NET Server SDK.

## Overview

This application connects to a FeatBit instance and continuously monitors the value of a specified feature flag. It logs all flag values and detects changes in real-time.

## Features

- **Real-time Monitoring**: Continuously polls feature flag values
- **Change Detection**: Identifies and logs when flag values change
- **Configurable Intervals**: Adjustable polling frequency
- **Server SDK**: Uses FeatBit's official .NET Server SDK

## Prerequisites

- .NET 8.0 SDK or later
- Access to a FeatBit instance
- Valid environment secret from FeatBit

## Configuration

Copy `.env.example` to `.env` and configure:

```bash
FEATBIT_ENV_SECRET=your-env-secret
FEATBIT_STREAMING_URI=ws://localhost:5100
FEATBIT_EVENT_URI=http://localhost:5100
FEATBIT_FLAG_KEY=test-flag
FEATBIT_USER_KEY=test-user
CHECK_INTERVAL_MS=5000
```

### Configuration Parameters

- `FEATBIT_ENV_SECRET`: Your FeatBit environment secret key
- `FEATBIT_STREAMING_URI`: WebSocket URI for real-time updates
- `FEATBIT_EVENT_URI`: HTTP URI for events
- `FEATBIT_FLAG_KEY`: The feature flag key to monitor
- `FEATBIT_USER_KEY`: User context for flag evaluation
- `CHECK_INTERVAL_MS`: Polling interval in milliseconds

## Building

```bash
cd src/DotNetServerMonitor/DotNetServerMonitor
dotnet build
```

## Running

```bash
dotnet run
```

Or with custom configuration:

```bash
export FEATBIT_ENV_SECRET="your-secret"
export FEATBIT_FLAG_KEY="my-flag"
dotnet run
```

## Output

The monitor outputs:
- Startup configuration
- Connection status
- Current flag values at each interval
- Flag value changes with timestamps

Example output:
```
FeatBit .NET Server Monitor Starting...
Configuration:
  Streaming URI: ws://localhost:5100
  Event URI: http://localhost:5100
  Flag Key: test-flag
  User Key: test-user
FeatBit client initialized successfully.

Monitoring flag 'test-flag' every 5000ms...
Press Ctrl+C to exit.

[2026-01-16 14:30:00.123] Flag: 'test-flag' = 'value-a'
[2026-01-16 14:30:05.234] Flag: 'test-flag' = 'value-a'
[2026-01-16 14:30:10.345] FLAG CHANGED: 'test-flag' = 'value-b' (previous: 'value-a')
```

## Deployment

### As a Service (Linux)

Create a systemd service file `/etc/systemd/system/featbit-dotnet-monitor.service`:

```ini
[Unit]
Description=FeatBit .NET Server Monitor
After=network.target

[Service]
Type=simple
User=featbit
WorkingDirectory=/opt/featbit-monitor/src/DotNetServerMonitor/DotNetServerMonitor
Environment="FEATBIT_ENV_SECRET=your-secret"
Environment="FEATBIT_FLAG_KEY=test-flag"
ExecStart=/usr/bin/dotnet run
Restart=always

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl enable featbit-dotnet-monitor
sudo systemctl start featbit-dotnet-monitor
```

## Troubleshooting

### Connection Issues
- Verify FeatBit service is running
- Check firewall rules
- Ensure correct URIs and ports

### Flag Not Found
- Verify flag key exists in FeatBit
- Check environment secret is correct
- Ensure user has access to the flag

## License

MIT
