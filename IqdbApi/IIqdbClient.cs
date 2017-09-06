using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IqdbApi.Models;

namespace IqdbApi
{
    public interface IIqdbClient : IDisposable
    {
        /// <summary>
        /// Search similar images by web url.
        /// </summary>
        /// <param name="url">Url of the image for search.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A search results.</returns>
        Task<SearchResult> SearchUrl(string url, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Search similar images by file.
        /// </summary>
        /// <param name="fileStream">Input file to search.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A search results.</returns>
        Task<SearchResult> SearchFile(Stream fileStream, CancellationToken cancellationToken = default(CancellationToken));
    }
}
