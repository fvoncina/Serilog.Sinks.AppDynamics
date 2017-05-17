using System.Net.Http;
using System.Threading.Tasks;
using Serilog.Sinks.AppDynamics.Sinks.AppDynamics;

namespace Serilog.Sinks.AppDynamics
{
    internal class HttpClientWrapper : IHttpClient
    {
        private readonly HttpClient client;

        public HttpClientWrapper()
        {
            client = new HttpClient();
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return client.PostAsync(requestUri, content);
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}