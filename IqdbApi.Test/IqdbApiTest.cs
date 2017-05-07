using System;
using System.Linq;
using System.Threading.Tasks;
using IqdbApi.Enums;
using IqdbApi.Exceptions;
using IqdbApi.Models;
using NUnit.Framework;

namespace IqdbApi.Test
{
    [TestFixture]
    public class IqdbApiTest
    {
        [Test]
        // found
        [TestCase("https://files.yande.re/sample/5aa94d7e5e8a7209f889459ef9f2ab9e/yande.re 392121 sample amatsukaze_(kancolle) kantai_collection pantsu rensouhou-kun see_through seifuku snow_is_ stockings string_panties thighhighs wet.jpg")]
        // not found
        [TestCase("https://cs541604.userapi.com/c836722/v836722677/342ba/JKnecCszdCM.jpg")]
        public async Task SearchUrlSuccess(string url)
        {
            // arrange
            IIqdbApi api = new IqdbApi();

            // act
            var result = await api.SearchUrl(url);

            // assert
            Assert.NotNull(result);

            if (result.IsFound)
            {
                Assert.NotNull(result.Matches);
                Assert.Greater(result.Matches.Count(match => match.MatchType == MatchType.Additional), 1);
                Assert.Greater(result.Matches.Count(match => match.MatchType == MatchType.Other), 0);
                Assert.AreEqual(result.Matches.Count(match => match.MatchType == MatchType.Best), 1);

            }
            else
            {
                Assert.AreEqual(result.Matches.Count(match => match.MatchType == MatchType.Best), 0);
                Assert.AreEqual(result.Matches.Count(match => match.MatchType == MatchType.Additional), 0);
            }

            Assert.Greater(result.SearchedImagesCount, 0);
            Assert.Greater(result.SearchedInSeconds, 0);
            
            Assert.NotNull(result.YourImage);
            Assert.IsFalse(String.IsNullOrWhiteSpace(result.YourImage.Name));
            Assert.IsFalse(String.IsNullOrWhiteSpace(result.YourImage.PreviewUrl));
            Assert.IsFalse(String.IsNullOrWhiteSpace(result.YourImage.Size));

            foreach (var match in result.Matches)
            {
                Assert.Greater(match.Similarity, 0);
                Assert.AreNotEqual(match.Resolution, default(Resolution));

                Assert.IsFalse(String.IsNullOrWhiteSpace(match.PreviewUrl));
                Assert.IsFalse(String.IsNullOrWhiteSpace(match.Url));
            }
        }

        [Test]
        public void SearchUrlException()
        {
            // arrange
            IIqdbApi api = new IqdbApi();

            // act
            // assert

            Assert.ThrowsAsync<NotImageException>(() => api.SearchUrl("https://yande.re/favicon.ico"));
            Assert.ThrowsAsync<NotImageException>(() => api.SearchUrl("https://iqdb.org/default.css"));

            Assert.ThrowsAsync<ArgumentException>(() => api.SearchUrl(""));
            Assert.ThrowsAsync<ArgumentException>(() => api.SearchUrl("  "));
            Assert.ThrowsAsync<ArgumentNullException>(() => api.SearchUrl(null));


            // http failed
            Assert.ThrowsAsync<HttpRequestFailed>(() => api.SearchUrl(" afsdfqewr1234 "));

            // size
            Assert.ThrowsAsync<ImageTooLagreException>(() => api.SearchUrl("https://files.yande.re/image/8f8c1f35e4ca613487dda4ea2d282077/yande.re%20377441%20armor%20bodysuit%20gun%20heroes_of_the_storm%20mecha%20monster%20overwatch%20sword%20tattoo%20tracer%20weapon%20zarya.jpg"));

            // resolution
            Assert.ThrowsAsync<ImageTooLagreException>(() => api.SearchUrl("https://files.yande.re/image/cd73e77b015a257fa807afdc3043cbc0/yande.re%20277274%20aoki_hagane_no_arpeggio%20ass%20bikini%20morita_kazuaki%20panty_pull%20stick_poster%20swimsuits%20takao_%28aoki_hagane_no_arpeggio%29.jpg"));

        }
    }
}
