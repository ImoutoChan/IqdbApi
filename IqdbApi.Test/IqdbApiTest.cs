using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IqdbApi.Enums;
using IqdbApi.Exceptions;
using IqdbApi.Models;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace IqdbApi.Test
{
    [TestFixture]
    public class IqdbApiTest
    {
        private static readonly IEnumerable<ITestCaseData> _searchUrlSuccessTestCases;

        static IqdbApiTest()
        {
            var urls = new[]
            {
                "https://pp.userapi.com/c639830/v639830431/11db4/peMZxfCdiko.jpg",
                "https://pp.userapi.com/c626224/v626224431/6291e/z3mBzT9q104.jpg",
                "https://pp.userapi.com/c636425/v636425431/4d13c/R20-IOXNFds.jpg",
                "https://pp.userapi.com/c626224/v626224431/5f1bf/v4xwKIUIaaM.jpg",
                "https://pp.userapi.com/c626224/v626224431/5d89e/vdcpu2qHqWc.jpg",
                "https://pp.userapi.com/c626224/v626224431/5dc6b/QqJkFerqHso.jpg",
                "https://pp.userapi.com/c626224/v626224431/5f89c/dGIwli6c92s.jpg",
                "https://pp.userapi.com/c639830/v639830431/102ad/KknV_pMoh1A.jpg",
                "https://pp.userapi.com/c626224/v626224431/5decf/VvTpyU8A0Zc.jpg",
                "https://files.yande.re/sample/5aa94d7e5e8a7209f889459ef9f2ab9e/yande.re 392121 sample amatsukaze_(kancolle) kantai_collection pantsu rensouhou-kun see_through seifuku snow_is_ stockings string_panties thighhighs wet.jpg",
                "https://cs541604.userapi.com/c836722/v836722677/342ba/JKnecCszdCM.jpg"
            };

            byte counter = 0;
            _searchUrlSuccessTestCases 
                = urls.Select(url => new TestCaseData(url)
                {
                    TestName = $"SearchUrlSuccess {counter++}"
                } as ITestCaseData);


        }

        [TestCaseSource(nameof(_searchUrlSuccessTestCases))]
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

        [TestCase("Resources/9cc122fe5884a090d1dfe6832b8ed19f.jpg")]
        public async Task SearchFileSuccess(string filename)
        {
            // arrange
            IIqdbApi api = new IqdbApi();
            SearchResult result;

            // act
            using (var fs = new FileStream(filename, FileMode.Open))
            {
                result = await api.SearchFile(fs);
            }

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
            
            foreach (var match in result.Matches)
            {
                Assert.Greater(match.Similarity, 0);
                Assert.AreNotEqual(match.Resolution, default(Resolution));

                Assert.IsFalse(String.IsNullOrWhiteSpace(match.PreviewUrl));
                Assert.IsFalse(String.IsNullOrWhiteSpace(match.Url));
            }
        }

        [Test]
        public void SearchFileException()
        {
            // arrange
            IIqdbApi api = new IqdbApi();

            // act
            // assert

            var fs = new FileStream("Resources/favicon.ico", FileMode.Open);

            Assert.ThrowsAsync<InvalidFileFormatException>(() => api.SearchFile(fs));


            Assert.ThrowsAsync<ArgumentNullException>(() => api.SearchFile(null));

            fs.Dispose();
            fs = new FileStream("Resources/large.jpg", FileMode.Open);

            Assert.ThrowsAsync<ImageTooLagreException>(() => api.SearchFile(fs));

            fs.Dispose();
        }

        [Test]
        public async Task RateLimiter()
        {
            async Task<DateTimeOffset> GetImage(string url)
            {
                var api = new IqdbApi();
                var result = await api.SearchUrl(url);
                return DateTimeOffset.UtcNow;
            }

            var urls = new[]
            {
                "https://pp.userapi.com/c639830/v639830431/102ad/KknV_pMoh1A.jpg",
                "https://pp.userapi.com/c626224/v626224431/5dc6b/QqJkFerqHso.jpg",
                "https://pp.userapi.com/c626224/v626224431/5decf/VvTpyU8A0Zc.jpg",
                "https://pp.userapi.com/c626224/v626224431/5f1bf/v4xwKIUIaaM.jpg",
                "https://pp.userapi.com/c636425/v636425431/4d13c/R20-IOXNFds.jpg"
            };

            var tasks = urls.Select(url => GetImage(url)).ToArray();


            var startTime = DateTimeOffset.UtcNow;
            await Task.WhenAll(tasks);

            var taskDurations = tasks.Select(task => (task.Result - startTime).Duration().TotalSeconds).ToList();

            var holes = new Dictionary<int, double>();
            for (int i = 5; i <= tasks.Length * 5; i += 5)
            {
                holes.Add(i, taskDurations.Where(x => x > i).Min());
            }
            
            foreach (var hole in holes)
            {
                Assert.NotNull(hole.Value);
            }

            for (int i = 0; i < holes.Count; i++)
            {
                for (int j = 0; j < holes.Count; j++)
                {
                    if (i == j)
                        continue;

                    Assert.AreNotEqual(holes.ElementAt(i), holes.ElementAt(j));
                }
            }
        }
    }
}
