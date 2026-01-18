/**
 * Send a message to Feishu webhook
 * @param {Object} env - Environment configuration
 * @param {string} message
 * @returns {Promise<boolean>}
 */
export async function sendMessage(env, message) {
  const webhookUrl = env.FEISHU_WEBHOOK_URL;

  if (!webhookUrl) {
    console.warn('Feishu webhook URL is not configured, skipping notification');
    return false;
  }

  const payload = {
    msg_type: 'text',
    content: {
      text: message
    }
  };

  try {
    const response = await fetch(webhookUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });

    if (response.ok) {
      console.log('Successfully sent message to Feishu');
      return true;
    } else {
      const content = await response.text();
      console.error(
        `Failed to send message to Feishu. Status: ${response.status}, Content: ${content}`
      );
      return false;
    }
  } catch (error) {
    console.error('HTTP error while sending message to Feishu:', error);
    return false;
  }
}

/**
 * Send feature flag monitoring result to Feishu
 * @param {Object} env - Environment configuration
 * @param {string} featureFlagKey
 * @param {string} targetStatus
 * @param {boolean} toggleSuccess
 * @param {boolean|null} verificationSuccess
 * @returns {Promise<boolean>}
 */
export async function sendMonitoringResult(env, featureFlagKey, targetStatus, toggleSuccess, verificationSuccess = null) {
  const statusEmoji = toggleSuccess ? '✅' : '❌';
  const verificationEmoji =
    verificationSuccess === true ? '✅' :
      verificationSuccess === false ? '❌' :
        '⏭️';

  const verificationText =
    verificationSuccess === true ? 'Verified' :
      verificationSuccess === false ? 'Failed' :
        'Skipped';

  const message = `🔔 Feature Flag Monitor Report

Flag Key: ${featureFlagKey}
Target Status: ${targetStatus}
Toggle Result: ${statusEmoji} ${toggleSuccess ? 'Success' : 'Failed'}
Verification: ${verificationEmoji} ${verificationText}
Time: ${new Date().toISOString().replace('T', ' ').slice(0, 19)} UTC`;

  return await sendMessage(env, message);
}

/**
 * Send hourly health status report to Feishu
 * @param {Object} env - Environment configuration
 * @returns {Promise<boolean>}
 */
export async function sendHealthStatusReport(env) {
  const message = `✅ Monitoring Service Health Check

The monitoring tool has been running normally for the past hour with no abnormal evaluations detected.

Time: ${new Date().toISOString().replace('T', ' ').slice(0, 19)} UTC`;

  return await sendMessage(env, message);
}
