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
    public class IqdbApi : IIqdbApi
    {
        private static readonly int _waitSeconds = 5100;
        private static readonly string _baseAddress = @"https://iqdb.org/";
        private static readonly SemaphoreSlim _httpClientSemaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly HttpClient _httpClient = new HttpClient();

        public IqdbApi()
        {
            _httpClient.BaseAddress = new Uri(_baseAddress);
        }

        private async Task<HttpClient> GetClient(CancellationToken cancellationToken)
        {
            await _httpClientSemaphoreSlim.WaitAsync(cancellationToken);

            await Task.Delay(_waitSeconds, cancellationToken);

            _httpClientSemaphoreSlim.Release();

            return _httpClient;
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
            
            var client = await GetClient(cancellationToken);

            var httpResponse = await client.GetAsync($"?url={url}", cancellationToken);

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


            var form = new MultipartFormDataContent();

            form.Add(new StringContent("8388608"), "MAX_FILE_SIZE");

            for (int i = 1; i <= 13; i++)
            {
                if (new[] {7, 8, 9, 12}.Contains(i))
                {
                    continue;
                }

                form.Add(new StringContent(i.ToString()), "service[]");
            }
            
            form.Add(new StreamContent(fileStream), "file", "image.jpg");
            form.Add(new StringContent(String.Empty), "url");
            

            var client = await GetClient(cancellationToken);
            var response = await client.PostAsync("/", form, cancellationToken);

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
    }
}