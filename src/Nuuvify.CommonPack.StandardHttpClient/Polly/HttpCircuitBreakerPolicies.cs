using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace Nuuvify.CommonPack.StandardHttpClient.Polly
{
    public static class HttpCircuitBreakerPolicies
    {
        public static AsyncCircuitBreakerPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy(ILogger logger, ICircuitBreakerPolicyConfig circuitBreakerPolicyConfig)
        {
            return HttpPolicyBuilders.GetBaseBuilder()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: circuitBreakerPolicyConfig.RetryCount + 1,
                    durationOfBreak: TimeSpan.FromMilliseconds(circuitBreakerPolicyConfig.BreakDurationMilliSeconds),
                    onBreak: (responseMessage, breakDuration, context) =>
                    {
                        OnHttpBreak(responseMessage, breakDuration, circuitBreakerPolicyConfig.RetryCount, context, logger);
                    },
                    onReset: (context) =>
                    {
                        OnHttpReset(context, logger);
                    });
        }

        private static void OnHttpBreak(DelegateResult<HttpResponseMessage> responseMessage, TimeSpan breakDuration, int retryCount, Context context, ILogger logger)
        {
            var serviceBreak = context.GetServiceName() ?? responseMessage?.Result?.ReasonPhrase;
            

            var messageLog = $"{nameof(GetHttpCircuitBreakerPolicy)}";
            logger.LogWarning(messageLog + " Service: {0} shutdown during: {1} after: {2} failed retries.", serviceBreak, breakDuration, retryCount);

        }

        private static void OnHttpReset(Context context, ILogger logger)
        {
            var serviceBreak = context.GetServiceName();
            context.Remove(PollyCustomExtensions.ServiceNameKey);

            var messageLog = $"{nameof(GetHttpCircuitBreakerPolicy)}";
            logger.LogInformation(messageLog + " Service restarted: {0} ", serviceBreak);
        }

    }
}