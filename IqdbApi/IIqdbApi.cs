using System.IO;
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

        /// <summary>
        /// Search images by file.
        /// </summary>
        /// <param name="fileStream">Iput file to search.</param>
        /// <returns>Search results.</returns>
        Task<SearchResult> SearchFile(Stream fileStream);
    }
}
