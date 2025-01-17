using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Nuuvify.CommonPack.StandardHttpClient.Polly
{
    public static class HttpRetryPolicies
    {
        public static AsyncRetryPolicy<HttpResponseMessage> GetHttpResponseRetryPolicy(HttpRequestMessage request, ILogger logger, IRetryPolicyConfig retryPolicyConfig)
        {
            int retryNum = 0;

            return HttpPolicyBuilders.GetBaseBuilder()
                .WaitAndRetryAsync(
                    retryCount: retryPolicyConfig.RetryCount,
                    sleepDurationProvider: attemp => PollyHelpers.ComputeDuration(attemp),
                    onRetryAsync: async (message, retrySleep, context) =>
                    {
                        retryNum++;
                        await OnHttpRetry(message, request, retrySleep, retryNum, retryPolicyConfig.RetryCount, context, logger);

                        if (retryNum > retryPolicyConfig.RetryCount) retryNum = 0;
                    });
        }

        private static async Task OnHttpRetry(DelegateResult<HttpResponseMessage> message, HttpRequestMessage request, TimeSpan retrySleep, int retryNum, int retryTotal, Context context, ILogger logger)
        {

            var messageLog = $"{nameof(GetHttpResponseRetryPolicy)} Request failed with StatusCode: {message?.Result?.StatusCode}. Waiting retrySleep: {retrySleep} before next retry. Retry attempt {retryNum}/{retryTotal} Context CorrelationId: {context.CorrelationId} Request: {request?.RequestUri}";

            var serviceBreak = request?.RequestUri.ToString();
            context.SetServiceName(serviceBreak);
            request.SetPolicyExecutionContext(context);


            if (message?.Exception?.Message != null)
            {

                logger.LogWarning(messageLog + " - Request failed because network failure: {0}", message?.Exception?.Message);
            }
            else
            {
                logger.LogWarning(messageLog);
            }

            await Task.CompletedTask;
        }



    }
}