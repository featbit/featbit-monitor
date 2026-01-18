import { executeMonitor } from './monitor.js';

export default {
  /**
   * Scheduled event handler for Cloudflare Workers cron trigger
   * @param {ScheduledEvent} event
   * @param {Env} env
   * @param {ExecutionContext} ctx
   */
  async scheduled(event, env, ctx) {
    console.log('Cron trigger executed at:', new Date(event.scheduledTime).toISOString());
    
    // Use waitUntil to ensure the async operation completes
    ctx.waitUntil(executeMonitor(env));
  },

  /**
   * HTTP handler for manual testing/debugging
   * @param {Request} request
   * @param {Env} env
   * @param {ExecutionContext} ctx
   */
  async fetch(request, env, ctx) {
    const url = new URL(request.url);
    
    // Debug endpoint
    if (url.pathname === '/debug' || url.pathname === '/') {
      console.log('HTTP trigger - Debug execution started');
      
      try {
        await executeMonitor(env);
        return new Response('Feature Flag Monitor executed successfully', {
          status: 200,
          headers: { 'Content-Type': 'text/plain' }
        });
      } catch (error) {
        console.error('Error during execution:', error);
        return new Response(`Error: ${error.message}`, {
          status: 500,
          headers: { 'Content-Type': 'text/plain' }
        });
      }
    }
    
    return new Response('Feature Flag Monitor\nEndpoints:\n  GET / or /debug - Manual trigger', {
      status: 200,
      headers: { 'Content-Type': 'text/plain' }
    });
  }
};
