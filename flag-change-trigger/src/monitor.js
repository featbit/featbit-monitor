import { getFeatureFlags, toggleFeatureFlag } from './services/featbit-service.js';
import { verifyFeatureFlagStatus } from './services/verification-service.js';
import { sendMonitoringResult, sendHealthStatusReport } from './services/feishu-service.js';

/**
 * Sleep for specified milliseconds
 * @param {number} ms - Milliseconds to sleep
 * @returns {Promise<void>}
 */
function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * Execute feature flag monitoring
 * @param {Object} env - Environment configuration
 * @returns {Promise<void>}
 */
export async function executeMonitor(env) {
  console.log('Feature Flag Monitor function executed at:', new Date().toISOString());

  try {
    // Step 1: Get all feature flags from the environment
    const flagsResponse = await getFeatureFlags(env);

    if (!flagsResponse?.data?.items || flagsResponse.data.items.length === 0) {
      console.warn('No feature flags found in environment');
      return;
    }

    // Step 2: Filter for boolean-type feature flags only
    const booleanFlags = flagsResponse.data.items.filter(flag => {
      // FeatBit boolean flags have variationType as 'boolean' or returnType as 'boolean'
      return flag.variationType === 'boolean' || 
             flag.returnType === 'boolean' ||
             (typeof flag.isEnabled === 'boolean');
    });

    if (booleanFlags.length === 0) {
      console.warn('No boolean-type feature flags found in environment');
      return;
    }

    console.log(`Found ${booleanFlags.length} boolean-type feature flags out of ${flagsResponse.data.items.length} total flags`);

    // Step 3: Randomly select a boolean feature flag
    const randomIndex = Math.floor(Math.random() * booleanFlags.length);
    const selectedFlag = booleanFlags[randomIndex];

    // Step 3: Toggle to opposite status
    const targetStatus = (!selectedFlag.isEnabled).toString().toLowerCase();

    console.log(
      `Randomly selected feature flag: '${selectedFlag.key}'`,
      `(Current: ${selectedFlag.isEnabled}, Target: ${targetStatus})`
    );

    const toggleResult = await toggleFeatureFlag(
      env,
      selectedFlag.key,
      targetStatus
    );

    if (!toggleResult) {
      console.error(`Failed to toggle feature flag '${selectedFlag.key}'`);
      await sendMonitoringResult(
        env,
        selectedFlag.key,
        targetStatus,
        false,
        null
      );
      return;
    }

    console.log(
      `Successfully toggled feature flag '${selectedFlag.key}' to status: ${targetStatus}`
    );

    // Wait a moment for the change to propagate
    await sleep(2000);

    // Step 4: Verify the feature flag status
    const verificationResult = await verifyFeatureFlagStatus(
      env,
      selectedFlag.key,
      targetStatus
    );

    if (verificationResult) {
      console.log(
        `Feature flag '${selectedFlag.key}' status verified successfully. Status matches: ${targetStatus}`
      );
      
      // Send hourly health report at xx:15-17 window (allows for execution delays)
      const now = new Date();
      const minutes = now.getMinutes();
      const shouldSendStatusReport = minutes >= 15 && minutes <= 17;
      
      if (shouldSendStatusReport) {
        console.log('Sending hourly health status report (xx:15-17 window)');
        await sendHealthStatusReport(env);
      } else {
        console.log('Verification successful, skipping notification (not in hourly report window)');
      }
    } else {
      console.warn(
        `Feature flag '${selectedFlag.key}' status verification failed. Expected: ${targetStatus}, but verification did not match`
      );
      
      // Always send alert notification when verification fails
      console.log('Sending alert notification due to verification failure');
      await sendMonitoringResult(
        env,
        selectedFlag.key,
        targetStatus,
        true,
        verificationResult
      );
    }
  } catch (error) {
    console.error('Error during feature flag monitoring:', error);
    throw error;
  }
}
