using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IqdbApi.Exceptions;
using IqdbApi.Models;

namespace IqdbApi
{
    public class IqdbClient : IIqdbClient
    {
        private readonly SemaphoreSlim _httpClientSemaphoreSlim = new SemaphoreSlim(1);
        private readonly int _waitMilliseconds;
        private readonly HttpClient _httpClient;

        private DateTimeOffset _lastRequestTime = DateTimeOffset.Now.AddDays(-1);

        public IqdbClient(
            HttpMessageHandler httpMessageHandler = null,
            int waitMilliseconds = 5100,
            bool useHttps = true)
        {
            var handler = httpMessageHandler ?? new HttpClientHandler();

            var scheme = useHttps ? "https" : "http";

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri($"{scheme}://www.iqdb.org/")
            };

            _waitMilliseconds = waitMilliseconds;
        }

        public async Task<SearchResult> SearchUrl(
            string url,
            CancellationToken cancellationToken = default)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException(nameof(url));


            var httpResponse = await UseClient(
                async (httpClient, cT) => await httpClient.GetAsync($"?url={url}", cT),
                cancellationToken);

            httpResponse.EnsureSuccessStatusCode();
            var html = await httpResponse.Content.ReadAsStringAsync();

            var parser = new SearchResultParser();
            return parser.ParseResult(html);
        }

        public async Task<SearchResult> SearchFile(
            Stream fileStream,
            CancellationToken cancellationToken = default)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
            if (fileStream.Length > 8_388_608) throw new ImageTooLargeException();

            var form = GetFromDataContent(fileStream);

            var response = await UseClient(
                async (httpClient, cT) => await httpClient.PostAsync("/", form, cT),
                cancellationToken);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                if (response.StatusCode == HttpStatusCode.RequestEntityTooLarge)
                    throw new ImageTooLargeException(innerException: e);

                throw;
            }

            var html = await response.Content.ReadAsStringAsync();

            var parser = new SearchResultParser();
            return parser.ParseResult(html);
        }

        public void Dispose()
        {
            _httpClientSemaphoreSlim?.Dispose();
            _httpClient.Dispose();
        }

        private async Task<T> UseClient<T>(
            Func<HttpClient, CancellationToken, Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            await _httpClientSemaphoreSlim.WaitAsync(cancellationToken);

            var timePassed = _lastRequestTime - DateTimeOffset.UtcNow;
            var timePassedMilliseconds = timePassed.Duration().TotalMilliseconds;
            var delayDuration = _waitMilliseconds - Convert.ToInt32(timePassedMilliseconds);

            if (delayDuration > 0) await Task.Delay(delayDuration, cancellationToken);

            try
            {
                return await action(_httpClient, cancellationToken);
            }
            finally
            {
                _lastRequestTime = DateTimeOffset.UtcNow;
                _httpClientSemaphoreSlim.Release();
            }
        }

        private static MultipartFormDataContent GetFromDataContent(Stream fileStream)
        {
            var form = new MultipartFormDataContent();

            form.Add(new StringContent("8388608"), "MAX_FILE_SIZE");

            for (var i = 1; i <= 13; i++)
            {
                if (new[] {7, 8, 9, 12}.Contains(i)) continue;

                form.Add(new StringContent(i.ToString()), "service[]");
            }

            form.Add(new StreamContent(fileStream), "file", "image.jpg");
            form.Add(new StringContent(string.Empty), "url");

            return form;
        }
    }
}
