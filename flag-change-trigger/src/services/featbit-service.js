/**
 * Get all feature flags from the specified environment
 * @param {Object} env - Environment configuration
 * @returns {Promise<Object|null>}
 */
export async function getFeatureFlags(env) {
  const baseUrl = env.FEATBIT_BASE_URL || 'https://app-api.featbit.co/api/v1';
  const envId = env.FEATBIT_ENV_ID;
  const personalToken = env.FEATBIT_PERSONAL_TOKEN?.trim();

  console.log('Debug info:', {
    hasEnvId: !!envId,
    hasToken: !!personalToken,
    tokenLength: personalToken?.length,
    tokenPrefix: personalToken?.substring(0, 10)
  });

  if (!envId) {
    console.error('Environment ID is required');
    return null;
  }

  if (!personalToken) {
    console.error('Personal token is required');
    return null;
  }

  const url = `${baseUrl}/envs/${envId}/feature-flags`;

  try {
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Authorization': personalToken,
        'Content-Type': 'application/json'
      }
    });

    if (response.ok) {
      const result = await response.json();
      console.log(
        `Successfully retrieved ${result?.data?.totalCount ?? 0} feature flags from environment ${envId}`
      );
      return result;
    } else {
      const content = await response.text();
      console.error(
        `Failed to get feature flags. Status: ${response.status}, Content: ${content}`
      );
      return null;
    }
  } catch (error) {
    console.error('HTTP error while getting feature flags:', error);
    return null;
  }
}

/**
 * Toggle the status of a specified feature flag
 * @param {Object} env - Environment configuration
 * @param {string} featureFlagKey
 * @param {string} targetStatus - "true" or "false"
 * @returns {Promise<boolean>}
 */
export async function toggleFeatureFlag(env, featureFlagKey, targetStatus) {
  const baseUrl = env.FEATBIT_BASE_URL || 'https://app-api.featbit.co/api/v1';
  const envId = env.FEATBIT_ENV_ID;
  const personalToken = env.FEATBIT_PERSONAL_TOKEN?.trim();

  if (!envId || !featureFlagKey) {
    console.error('Environment ID and Feature Flag Key are required');
    return false;
  }

  const url = `${baseUrl}/envs/${envId}/feature-flags/${featureFlagKey}/toggle/${targetStatus}`;

  try {
    const response = await fetch(url, {
      method: 'PUT',
      headers: {
        'Authorization': personalToken,
        'Content-Type': 'application/json'
      }
    });

    if (response.ok) {
      console.log(`Successfully toggled feature flag '${featureFlagKey}' to ${targetStatus}`);
      return true;
    } else {
      const content = await response.text();
      console.error(
        `Failed to toggle feature flag '${featureFlagKey}'. Status: ${response.status}, Content: ${content}`
      );
      return false;
    }
  } catch (error) {
    console.error(`HTTP error while toggling feature flag '${featureFlagKey}':`, error);
    return false;
  }
}
