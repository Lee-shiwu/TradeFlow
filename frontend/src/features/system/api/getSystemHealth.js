/**
 * @typedef {Object} HealthCheck
 * @property {string} name
 * @property {string} status
 * @property {number} durationMilliseconds
 */

/**
 * @typedef {Object} SystemHealth
 * @property {string} status
 * @property {string} service
 * @property {number} durationMilliseconds
 * @property {HealthCheck[]} checks
 * @property {string} traceId
 */

/**
 * @returns {Promise<SystemHealth>}
 */
export async function getSystemHealth() {
  const response = await fetch("/health/ready", {
    headers: {
      Accept: "application/json",
    },
  });
  if (!response.ok) {
    throw new Error(`Health check failed with HTTP ${response.status}.`);
  }
  return await response.json();
}
