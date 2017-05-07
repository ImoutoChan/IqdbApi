using System.Threading.Tasks;
using IqdbApi.Models;

namespace IqdbApi
{
    public interface IIqdbApi
    {
        /// <summary>
        /// Search images by web url.
        /// </summary>
        /// <param name="url">Url of image to search.</param>
        /// <returns>Search results.</returns>
        Task<SearchResult> SearchUrl(string url);
    }
}
