// NOTE: The featbit-js-client-sdk package is deprecated and has module resolution issues.
// This is a simplified demonstration implementation.
// For production use, refer to the latest FeatBit JavaScript SDK or use the REST API directly.

console.log('FeatBit JavaScript Client Monitor Starting...');

// Read configuration from environment variables
const envSecret = process.env.FEATBIT_ENV_SECRET || 'your-env-secret';
const streamingUri = process.env.FEATBIT_STREAMING_URI || 'ws://localhost:5100';
const eventUri = process.env.FEATBIT_EVENT_URI || 'http://localhost:5100';
const flagKey = process.env.FEATBIT_FLAG_KEY || 'test-flag';
const userKey = process.env.FEATBIT_USER_KEY || 'test-user';

console.log('Configuration:');
console.log(`  Streaming URI: ${streamingUri}`);
console.log(`  Event URI: ${eventUri}`);
console.log(`  Flag Key: ${flagKey}`);
console.log(`  User Key: ${userKey}`);

// Try to initialize FeatBit client
let fbClient = null;
let isUsingMockMode = false;

try {
    const { initialize } = require('featbit-js-client-sdk');
    
    const user = {
        keyId: userKey,
        name: userKey
    };

    const options = {
        api: streamingUri,
        appKey: envSecret,
        user: user
    };

    fbClient = initialize(options);

    fbClient.on('ready', () => {
        console.log('FeatBit client initialized successfully.\n');
        startMonitoring(fbClient);
    });

    fbClient.on('error', (error) => {
        console.error('FeatBit client error:', error);
        console.log('Switching to mock mode...\n');
        isUsingMockMode = true;
        startMockMonitoring();
    });

    // Listen for flag updates
    fbClient.on('update', (keys) => {
        if (keys.includes(flagKey)) {
            console.log(`[${new Date().toISOString().replace('T', ' ').substring(0, 23)}] FLAG UPDATE EVENT received for: ${flagKey}`);
        }
    });
} catch (error) {
    console.error('Error loading FeatBit SDK:', error.message);
    console.log('Running in MOCK MODE for demonstration purposes.\n');
    isUsingMockMode = true;
    startMockMonitoring();
}

function startMonitoring(client) {
    let lastValue = '';
    const checkInterval = parseInt(process.env.CHECK_INTERVAL_MS || '5000');
    
    console.log(`Monitoring flag '${flagKey}' every ${checkInterval}ms...`);
    console.log('Press Ctrl+C to exit.\n');
    
    setInterval(() => {
        try {
            const variation = client.variation(flagKey, 'default-value');
            const timestamp = new Date().toISOString().replace('T', ' ').substring(0, 23);
            
            if (variation !== lastValue) {
                console.log(`[${timestamp}] FLAG CHANGED: '${flagKey}' = '${variation}' (previous: '${lastValue}')`);
                lastValue = variation;
            } else {
                console.log(`[${timestamp}] Flag: '${flagKey}' = '${variation}'`);
            }
        } catch (error) {
            console.log(`Error evaluating flag: ${error.message}`);
        }
    }, checkInterval);
}

function startMockMonitoring() {
    let lastValue = '';
    let currentValue = 'mock-value-a';
    const checkInterval = parseInt(process.env.CHECK_INTERVAL_MS || '5000');
    
    console.log('='.repeat(60));
    console.log('RUNNING IN MOCK MODE');
    console.log('This demonstrates the monitoring behavior without a real FeatBit connection.');
    console.log('Flag values will alternate between mock-value-a and mock-value-b.');
    console.log('='.repeat(60));
    console.log();
    
    console.log(`Monitoring flag '${flagKey}' every ${checkInterval}ms...`);
    console.log('Press Ctrl+C to exit.\n');
    
    let counter = 0;
    setInterval(() => {
        try {
            // Simulate flag value changes every 5 iterations
            counter++;
            if (counter % 5 === 0) {
                currentValue = currentValue === 'mock-value-a' ? 'mock-value-b' : 'mock-value-a';
            }
            
            const timestamp = new Date().toISOString().replace('T', ' ').substring(0, 23);
            
            if (currentValue !== lastValue) {
                console.log(`[${timestamp}] FLAG CHANGED: '${flagKey}' = '${currentValue}' (previous: '${lastValue}')`);
                lastValue = currentValue;
            } else {
                console.log(`[${timestamp}] Flag: '${flagKey}' = '${currentValue}'`);
            }
        } catch (error) {
            console.log(`Error in mock monitoring: ${error.message}`);
        }
    }, checkInterval);
}

// Handle process termination
process.on('SIGINT', () => {
    console.log('\nShutting down...');
    if (fbClient && fbClient.close) {
        fbClient.close();
    }
    process.exit(0);
});

