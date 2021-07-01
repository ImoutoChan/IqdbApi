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
    public enum ProxyOption:byte
	{
        HttpsOnly,
        HttpOnly,
        TryHttpsFirst,
        TryHttpFirst
	}
    public class IqdbClient : IIqdbClient
    {
        static readonly Uri _httpUri = new Uri("http://www.iqdb.org/");
        static readonly Uri _httpsUri = new Uri("https://www.iqdb.org");
        private readonly Uri _firstUri;
        private readonly Uri _secondUri;
        private bool _usingFirst = true;
        private readonly bool _tryOnFailure = true;

        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _httpClientSemaphoreSlim = new SemaphoreSlim(1);
        private readonly int _waitMilliseconds;

        private DateTimeOffset _lastRequestTime = DateTimeOffset.Now.AddDays(-1);

        public IqdbClient(HttpMessageHandler httpMessageHandler = null, int waitMilliseconds = 5100, ProxyOption proxy = ProxyOption.TryHttpsFirst)
        {
            var handler = httpMessageHandler ?? new HttpClientHandler();

            _httpClient = new HttpClient(handler);
            switch (proxy)
            {
                case ProxyOption.HttpsOnly:
                    _firstUri = _httpsUri;
                    _tryOnFailure = false;
                    break;
                case ProxyOption.HttpOnly:
                    _firstUri = _httpUri;
                    _tryOnFailure = false;
                    break;
                case ProxyOption.TryHttpsFirst:
                    _firstUri = _httpsUri;
                    _secondUri = _httpUri;
                    break;
                case ProxyOption.TryHttpFirst:
                    _firstUri = _httpUri;
                    _secondUri = _httpsUri;
                    break;
            }
            _httpClient.BaseAddress = _firstUri;

            _waitMilliseconds = waitMilliseconds;
        }

        private async Task<T> UseClient<T>(
            Func<HttpClient, CancellationToken, Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            await _httpClientSemaphoreSlim.WaitAsync(cancellationToken);

            var timePassed = _lastRequestTime - DateTimeOffset.UtcNow;
            var timePassedMilliseconds = timePassed.Duration().TotalMilliseconds;
            var delayDuration = _waitMilliseconds - Convert.ToInt32(timePassedMilliseconds);

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

        public async Task<SearchResult> SearchUrl(
            string url,
            CancellationToken cancellationToken = default)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException(nameof(url));
            }


            var httpResponse = await UseClient(
                async (httpClient, cT) =>
                {
                    try
                    { return await httpClient.GetAsync($"?url={url}", cT); }
                    catch (Exception ex)
                    {
                        if (_tryOnFailure)
                        {
                            if (_usingFirst)
                                httpClient.BaseAddress = _secondUri;
                            else
                                httpClient.BaseAddress = _firstUri;
                            _usingFirst = !_usingFirst;
                            return await httpClient.GetAsync($"?url={url}", cT);
                        }
                        else
                            throw ex;
                    }
                },
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
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }
            if (fileStream.Length > 8_388_608)
            {
                throw new ImageTooLargeException();
            }

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
                {
                    throw new ImageTooLargeException(innerException: e);
                }

                throw;
            }

            var html = await response.Content.ReadAsStringAsync();

            var parser = new SearchResultParser();
            return parser.ParseResult(html);
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
            form.Add(new StringContent(string.Empty), "url");

            return form;
        }

        public void Dispose()
        {
            _httpClientSemaphoreSlim?.Dispose();
            _httpClient?.Dispose();
        }
    }
}
