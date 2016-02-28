using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SensorManager
{
    public class RestClient : IDisposable
    {
        private const string mimeType = "application/json";

        private HttpClient client;

        public RestClient(string restServiceUrl)
        {
            var handler = new HttpClientHandler { AllowAutoRedirect = true };
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            client = new HttpClient(handler)
            {
                BaseAddress = new Uri(restServiceUrl)
            };

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mimeType));
        }

        public Task PostAsync(string resource) => this.PostAsync<object>(resource, null);

        public async Task PostAsync<T>(string resource, T obj)
        {
            var response = await client.PostAsJsonAsync(resource, obj);
            response.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
