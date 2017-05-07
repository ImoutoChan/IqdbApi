using System;
using System.Net.Http;
using System.Threading.Tasks;
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
    }
}