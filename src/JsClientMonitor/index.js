const { initialize } = require('featbit-js-client-sdk');

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

// Initialize FeatBit client
const user = {
    keyId: userKey,
    name: userKey
};

const options = {
    api: streamingUri,
    appKey: envSecret,
    user: user
};

const fbClient = initialize(options);

fbClient.on('ready', () => {
    console.log('FeatBit client initialized successfully.\n');
    
    // Initial flag evaluation
    let lastValue = '';
    const checkInterval = parseInt(process.env.CHECK_INTERVAL_MS || '5000');
    
    console.log(`Monitoring flag '${flagKey}' every ${checkInterval}ms...`);
    console.log('Press Ctrl+C to exit.\n');
    
    // Monitor the flag in a loop
    setInterval(() => {
        try {
            const variation = fbClient.variation(flagKey, 'default-value');
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
    
    // Listen for flag updates
    fbClient.on('update', (keys) => {
        if (keys.includes(flagKey)) {
            console.log(`[${new Date().toISOString().replace('T', ' ').substring(0, 23)}] FLAG UPDATE EVENT received for: ${flagKey}`);
        }
    });
});

fbClient.on('error', (error) => {
    console.error('FeatBit client error:', error);
});

// Handle process termination
process.on('SIGINT', () => {
    console.log('\nShutting down...');
    fbClient.close();
    process.exit(0);
});
