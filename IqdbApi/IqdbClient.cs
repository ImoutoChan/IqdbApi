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
        private static readonly int _waitMilliseconds = 5100;
        private static readonly string _baseAddress = @"https://iqdb.org/";
        private static readonly SemaphoreSlim _httpClientSemaphoreSlim = new SemaphoreSlim(1);
        private static DateTimeOffset _lastRequestTime = DateTimeOffset.Now.AddDays(-1);

        private readonly HttpClient _httpClient = new HttpClient();

        public IqdbClient()
        {
            _httpClient.BaseAddress = new Uri(_baseAddress);
        }

        private async Task<T> UseClient<T>(Func<HttpClient, CancellationToken, Task<T>> action, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _httpClientSemaphoreSlim.WaitAsync(cancellationToken);

            var delayDuration = _waitMilliseconds - Convert.ToInt32((_lastRequestTime - DateTimeOffset.UtcNow).Duration().TotalMilliseconds);

            if (delayDuration > 0)
            {
                await Task.Delay(delayDuration, cancellationToken);
            }

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

        public async Task<SearchResult> SearchUrl(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException(nameof(url));
            }
            
            
            var httpResponse = await UseClient(async (httpClient, cT) => await httpClient.GetAsync($"?url={url}", cT),
                                                cancellationToken);

            httpResponse.EnsureSuccessStatusCode();
            var html = await httpResponse.Content.ReadAsStringAsync();

            var parser = new SearchResultParser();
            return parser.Parse(html);
        }

        public async Task<SearchResult> SearchFile(Stream fileStream, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }
            if (fileStream.Length > 8388608)
            {
                throw new ImageTooLagreException();
            }

            var form = GetFromDataContent(fileStream);
            
            var response = await UseClient(async (httpClient, cT) => await httpClient.PostAsync("/", form, cT),
                                           cancellationToken);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                if (response.StatusCode == HttpStatusCode.RequestEntityTooLarge)
                {
                    throw new ImageTooLagreException(innerException: e);
                }

                throw;
            }

            var html = await response.Content.ReadAsStringAsync();

            var parser = new SearchResultParser();
            return parser.Parse(html);
        }

        private static MultipartFormDataContent GetFromDataContent(Stream fileStream)
        {
            var form = new MultipartFormDataContent();

            form.Add(new StringContent("8388608"), "MAX_FILE_SIZE");

            for (int i = 1; i <= 13; i++)
            {
                if (new[] { 7, 8, 9, 12 }.Contains(i))
                {
                    continue;
                }

                form.Add(new StringContent(i.ToString()), "service[]");
            }

            form.Add(new StreamContent(fileStream), "file", "image.jpg");
            form.Add(new StringContent(String.Empty), "url");

            return form;
        }
    }
}