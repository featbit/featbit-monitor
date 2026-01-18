# FeatBit Feature Flag Monitor - Cloudflare Worker

This is a JavaScript version of the FeatBit Feature Flag Monitor, designed to run as a Cloudflare Worker with cron triggers.

## Features

- 🕐 **Scheduled Monitoring**: Runs automatically every 15 minutes via Cloudflare cron triggers
- 🎲 **Random Flag Selection**: Randomly selects a feature flag and toggles its status
- ✅ **Verification**: Verifies the flag status change through a custom verification endpoint
- 📢 **Feishu Notifications**: Sends monitoring results to Feishu (Lark) webhook
- 🔍 **HTTP Debug Endpoint**: Manual trigger endpoint for testing

## Project Structure

```
src/
├── index.js                          # Worker entry point
├── monitor.js                        # Main monitoring logic
└── services/
    ├── featbit-service.js            # FeatBit API interactions
    ├── verification-service.js       # Flag verification logic
    └── feishu-service.js             # Feishu notifications
```

## Prerequisites

- Node.js (v16 or higher)
- Cloudflare account
- Wrangler CLI

## Setup

### 1. Install Dependencies

```bash
npm install
```

### 2. Configure Environment Variables

For local development, create a `.dev.vars` file:

```bash
cp .dev.vars.example .dev.vars
```

Edit `.dev.vars` and fill in your values:

```env
FEATBIT_ENV_ID=your-environment-id
FEATBIT_PERSONAL_TOKEN=your-personal-token
VERIFICATION_BASE_URL=https://your-verification-endpoint.com
FEISHU_WEBHOOK_URL=https://open.feishu.cn/open-apis/bot/v2/hook/your-token
```

### 3. Configure Production Secrets

For production deployment, set secrets using Wrangler:

```bash
wrangler secret put FEATBIT_ENV_ID
wrangler secret put FEATBIT_PERSONAL_TOKEN
wrangler secret put VERIFICATION_BASE_URL
wrangler secret put FEISHU_WEBHOOK_URL
```

## Development

### Run Locally

```bash
npm run dev
```

This starts a local server. You can:
- Visit `http://localhost:8787/` or `http://localhost:8787/debug` to manually trigger the monitor
- Test cron triggers with `wrangler dev --test-scheduled`

### Test Scheduled Events

```bash
# Trigger scheduled event in local dev
curl "http://localhost:8787/__scheduled?cron=*+*+*+*+*"
```

## Deployment

### Deploy to Cloudflare Workers

```bash
npm run deploy
```

### View Logs

```bash
npm run tail
```

Or view logs in the Cloudflare dashboard.

## Configuration

### Cron Schedule

The cron schedule is defined in `wrangler.toml`:

```toml
[triggers]
crons = ["*/15 * * * *"]  # Every 15 minutes
```

You can modify this to your preferred schedule. Examples:
- `*/5 * * * *` - Every 5 minutes
- `0 * * * *` - Every hour
- `0 0 * * *` - Daily at midnight

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `FEATBIT_ENV_ID` | FeatBit environment ID | Yes |
| `FEATBIT_PERSONAL_TOKEN` | FeatBit personal access token | Yes |
| `FEATBIT_BASE_URL` | FeatBit API base URL | No (default: https://app-api.featbit.co/api/v1) |
| `VERIFICATION_BASE_URL` | Verification endpoint base URL | No |
| `VERIFICATION_TYPE` | Verification type parameter | No (default: "default") |
| `FEISHU_WEBHOOK_URL` | Feishu webhook URL for notifications | No |

## How It Works

1. **Scheduled Trigger**: Cloudflare triggers the worker every 15 minutes
2. **Fetch Flags**: Retrieves all feature flags from FeatBit environment
3. **Random Selection**: Randomly selects one flag
4. **Toggle Status**: Toggles the flag to the opposite status (true → false or false → true)
5. **Verification**: Calls verification endpoint to confirm the change
6. **Notification**: Sends result to Feishu webhook

## API Endpoints

### GET /debug or GET /

Manually trigger the monitoring process. Useful for testing.

**Response:**
```
Feature Flag Monitor executed successfully
```

## Monitoring

You can monitor your worker in several ways:

1. **Cloudflare Dashboard**: View metrics, logs, and cron executions
2. **Wrangler Tail**: Real-time log streaming with `npm run tail`
3. **Feishu Notifications**: Receive reports for each monitoring run

## Differences from .NET Version

- Uses native `fetch()` instead of HttpClient
- ES6 modules instead of C# classes
- Cloudflare cron triggers instead of Azure Timer triggers
- Environment variables instead of appsettings.json
- Simpler deployment process with Wrangler

## Troubleshooting

### Cron not triggering

- Verify cron schedule in `wrangler.toml`
- Check Cloudflare dashboard for cron execution history
- Ensure worker is deployed (not just in dev mode)

### Authentication errors

- Verify `FEATBIT_PERSONAL_TOKEN` is correct
- Check if token has required permissions
- Ensure token is set as secret in production

### Verification fails

- Confirm `VERIFICATION_BASE_URL` is accessible
- Check if verification endpoint returns expected format
- Verification is optional; monitor will work without it

## License

Same as the original .NET project.

## Migration Notes

This JavaScript version maintains feature parity with the original .NET Azure Functions version:
- ✅ Timer/Cron trigger
- ✅ HTTP debug endpoint
- ✅ FeatBit API integration
- ✅ Verification service
- ✅ Feishu notifications
- ✅ Logging
- ✅ Error handling
