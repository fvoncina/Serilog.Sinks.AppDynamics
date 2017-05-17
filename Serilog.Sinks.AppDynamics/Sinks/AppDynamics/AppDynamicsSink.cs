using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Sinks.AppDynamics.Helpers;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.AppDynamics.Sinks.AppDynamics
{
    internal class AppDynamicsSink : PeriodicBatchingSink
    {
        private const string ContentType = "application/json";
        private readonly string requestUri;
        private readonly long? eventBodyLimitBytes;
        private readonly ITextFormatter formatter;
        private IHttpClient client;

        public AppDynamicsSink(string requestUri,
            int batchPostingLimit,
            TimeSpan period,
            long? eventBodyLimitBytes,
            IHttpClient client)
            : base(batchPostingLimit, period)
        {
            this.requestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
            this.eventBodyLimitBytes = eventBodyLimitBytes;
            this.client = client ?? throw new ArgumentNullException(nameof(client));

            formatter = new JsonFormatter();
        }
        
        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            var payload = FormatPayload(events);
            var content = new StringContent(payload, Encoding.UTF8, ContentType);

            var result = await client
                .PostAsync(requestUri, content)
                .ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
                throw new LoggingFailedException($"Received failed result {result.StatusCode} when posting events to {requestUri}");
        }

        private string FormatPayload(IEnumerable<LogEvent> events)
        {
            var payload = new StringWriter();
            payload.Write("{\"events\":[");

            var delimStart = string.Empty;

            foreach (var logEvent in events)
            {
                var buffer = new StringWriter();
                formatter.Format(logEvent, buffer);

                if (string.IsNullOrEmpty(buffer.ToString()))
                {
                    continue;
                }

                var json = buffer.ToString();
                if (CheckEventBodySize(json))
                {
                    payload.Write(delimStart);
                    payload.Write(json);
                    delimStart = ",";
                }
            }

            payload.Write("]}");

            return payload.ToString();
        }

        private bool CheckEventBodySize(string json)
        {
            if (eventBodyLimitBytes.HasValue &&
                Encoding.UTF8.GetByteCount(json) > eventBodyLimitBytes.Value)
            {
                SelfLog.WriteLine(
                    "Event JSON representation exceeds the byte size limit of {0} set for this sink and will be dropped; data: {1}",
                    eventBodyLimitBytes,
                    json);

                return false;
            }

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;
            client?.Dispose();
            client = null;
        }
    }
}
