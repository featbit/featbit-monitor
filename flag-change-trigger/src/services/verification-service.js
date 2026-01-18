/**
 * Verify if the feature flag status matches the expected status
 * @param {Object} env - Environment configuration
 * @param {string} featureFlagKey
 * @param {string} expectedStatus - "true" or "false"
 * @returns {Promise<boolean>}
 */
export async function verifyFeatureFlagStatus(env, featureFlagKey, expectedStatus) {
  const baseUrl = env.VERIFICATION_BASE_URL;
  const type = env.VERIFICATION_TYPE || 'default';

  if (!baseUrl) {
    console.warn('Verification base URL is not configured, skipping verification');
    return true; // Consider it as passed if not configured
  }

  const url = `${baseUrl}/api/Features/${featureFlagKey}?type=${type}`;

  try {
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json'
      }
    });

    if (response.ok) {
      const content = await response.text();
      console.log('Verification API response:', content);

      // Try to parse as JSON first
      try {
        const data = JSON.parse(content);

        // Check various possible response formats
        if (data.status !== undefined) {
          const actualStatus = String(data.status).toLowerCase();
          return actualStatus === expectedStatus.toLowerCase();
        } else if (data.enabled !== undefined) {
          const actualStatus = String(data.enabled).toLowerCase();
          return actualStatus === expectedStatus.toLowerCase();
        } else if (typeof data === 'boolean') {
          const actualStatus = String(data).toLowerCase();
          return actualStatus === expectedStatus.toLowerCase();
        }
      } catch (jsonError) {
        // If not JSON, compare as plain text
        return content.trim().toLowerCase() === expectedStatus.toLowerCase();
      }

      console.warn('Could not find status field in response:', content);
      return false;
    } else {
      const errorContent = await response.text();
      console.error(
        `Verification API returned error: ${response.status}, Content: ${errorContent}`
      );
      return false;
    }
  } catch (error) {
    console.error('HTTP error while calling verification API:', error);
    return false;
  }
}
