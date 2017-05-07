using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IqdbApi.Exceptions;
using IqdbApi.Models;

namespace IqdbApi
{
    public class IqdbApi : IIqdbApi
    {
        private static readonly string BaseAddress = @"https://iqdb.org/";

        private readonly HttpClient _httpClient = new HttpClient();

        public IqdbApi()
        {
            _httpClient.BaseAddress = new Uri(BaseAddress);
        }

        public async Task<SearchResult> SearchUrl(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException(nameof(url));
            }

            var httpResponse = await _httpClient.GetAsync($"?url={url}");
            httpResponse.EnsureSuccessStatusCode();
            var html = await httpResponse.Content.ReadAsStringAsync();
            
            var parser = new SearchResultParser();
            return parser.Parse(html);
        }

        public async Task<SearchResult> SearchFile(Stream fileStream)
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

            var response = await _httpClient.PostAsync("/", form);

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