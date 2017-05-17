using System;
using Serilog.Configuration;
using Serilog.Sinks.AppDynamics.Sinks.AppDynamics;

namespace Serilog.Sinks.AppDynamics
{
    public static class LoggerSinkConfigurationExtensions
    {
        public static LoggerConfiguration AppDynamics(
            this LoggerSinkConfiguration sinkConfiguration,
            string requestUri,
            int batchPostingLimit = 1000,
            TimeSpan? periodBatchSyncs = null,
            long? eventBodyLimitBytes = 256 * 1024,
            IHttpClient httpClient = null)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (requestUri == null) throw new ArgumentNullException(nameof(requestUri));

            var sink = new AppDynamicsSink(requestUri, batchPostingLimit, periodBatchSyncs ?? TimeSpan.FromSeconds(5), eventBodyLimitBytes, httpClient ?? new HttpClientWrapper());

            return sinkConfiguration.Sink(sink);
        }
    }
}
