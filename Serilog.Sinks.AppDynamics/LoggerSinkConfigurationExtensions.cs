using System;
using Serilog.Configuration;
using Serilog.Sinks.AppDynamics.Helpers;
using Serilog.Sinks.AppDynamics.Sinks.AppDynamics;

namespace Serilog.Sinks.AppDynamics
{
    public static class LoggerSinkConfigurationExtensions
    {
        /// <summary>
        /// Sink configuration for AppDynamics integration.
        /// </summary>
        /// <param name="sinkConfiguration"></param>
        /// <param name="requestUri">AppDynamics API endpoint.</param>
        /// <param name="batchPostingLimit">The maximum number of events to be batched before sync.</param>
        /// <param name="periodBatchSyncs">Time to wait before batch occurs.</param>
        /// <param name="eventBodyLimitBytes">Max byte size before message is discarded. Default is 256KB</param>
        /// <param name="httpClient">HttpClient for issuing HTTP POST requests.</param>
        /// <returns>A SeriLog LoggerConfiguration extension hook.</returns>
        public static LoggerConfiguration AppDynamics(
            this LoggerSinkConfiguration sinkConfiguration,
            string requestUri,
            int batchPostingLimit = 1000,
            TimeSpan? periodBatchSyncs = null,
            long? eventBodyLimitBytes = 0x40000,
            IHttpClient httpClient = null)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (requestUri == null) throw new ArgumentNullException(nameof(requestUri));

            var sink = new AppDynamicsSink(requestUri, batchPostingLimit, periodBatchSyncs ?? TimeSpan.FromSeconds(5), eventBodyLimitBytes, httpClient ?? new HttpClientWrapper());

            return sinkConfiguration.Sink(sink);
        }
    }
}
