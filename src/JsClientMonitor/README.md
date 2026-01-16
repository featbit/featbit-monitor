# JavaScript Client Monitor

Long-running Node.js console application that monitors FeatBit feature flags using the JavaScript Client SDK.

## Overview

This application provides a client-side perspective on FeatBit feature flag behavior. It connects to FeatBit using the JavaScript Client SDK and monitors flag values in real-time with event-driven updates.

## Features

- **Real-time Monitoring**: Continuously monitors feature flag values
- **Event-Driven Updates**: Receives immediate notifications on flag changes
- **Change Detection**: Logs flag value changes with timestamps
- **Client SDK**: Uses FeatBit's official JavaScript Client SDK
- **Configurable Polling**: Adjustable check intervals

## Prerequisites

- Node.js 20.x or later
- npm or yarn
- Access to a FeatBit instance
- Valid environment secret from FeatBit

## Installation

```bash
cd src/JsClientMonitor
npm install
```

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

## Running

```bash
npm start
```

Or with custom configuration:

```bash
export FEATBIT_ENV_SECRET="your-secret"
export FEATBIT_FLAG_KEY="my-flag"
node index.js
```

## Output

The monitor outputs:
- Startup configuration
- Connection status
- Flag values at each polling interval
- Flag change events
- Update notifications from SDK

Example output:
```
FeatBit JavaScript Client Monitor Starting...
Configuration:
  Streaming URI: ws://localhost:5100
  Event URI: http://localhost:5100
  Flag Key: test-flag
  User Key: test-user
FeatBit client initialized successfully.

Monitoring flag 'test-flag' every 5000ms...
Press Ctrl+C to exit.

[2026-01-16 14:30:00.123] Flag: 'test-flag' = 'value-a'
[2026-01-16 14:30:10.234] FLAG UPDATE EVENT received for: test-flag
[2026-01-16 14:30:10.345] FLAG CHANGED: 'test-flag' = 'value-b' (previous: 'value-a')
```

## Deployment

### As a PM2 Process

Install PM2:
```bash
npm install -g pm2
```

Start the monitor:
```bash
pm2 start index.js --name featbit-js-monitor
```

Configure auto-restart:
```bash
pm2 startup
pm2 save
```

### As a Docker Container

Create `Dockerfile`:
```dockerfile
FROM node:20-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
CMD ["npm", "start"]
```

Build and run:
```bash
docker build -t featbit-js-monitor .
docker run -d \
  -e FEATBIT_ENV_SECRET=your-secret \
  -e FEATBIT_FLAG_KEY=test-flag \
  --name js-monitor \
  featbit-js-monitor
```

## Development

### Running in Development Mode

```bash
node index.js
```

### Adding Custom Logic

The monitor can be extended with custom handlers:

```javascript
fbClient.on('update', (keys) => {
    // Custom logic when flags update
    console.log('Flags updated:', keys);
});

fbClient.on('error', (error) => {
    // Custom error handling
    console.error('SDK error:', error);
});
```

## Troubleshooting

### SDK Deprecated Warning

The warning about `featbit-js-client-sdk` being deprecated is expected. The package is still functional for monitoring purposes. For production use, check for updated SDK packages.

### Connection Issues
- Verify FeatBit service is accessible
- Check network connectivity
- Ensure WebSocket connections are allowed

### Flag Not Found
- Verify flag key exists in FeatBit
- Check environment secret is correct
- Ensure flag is published and active

### Event Not Received
- Check WebSocket connection is established
- Verify streaming URI is correct
- Ensure flag updates are being published

## Performance

The JavaScript client is lightweight and suitable for:
- Development environments
- Integration testing
- Client-side behavior simulation
- Performance monitoring

## License

MIT
