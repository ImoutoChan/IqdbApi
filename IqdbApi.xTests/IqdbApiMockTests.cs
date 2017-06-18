using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IqdbApi.Enums;
using IqdbApi.Exceptions;
using IqdbApi.Models;
using RichardSzalay.MockHttp;
using Xunit;

namespace IqdbApi.xTests.IqdbApiTestContainer
{
    public class IqdbApiMockTests
    {
        public class TheSearchUrlMethod : IqdbApiRealTests
        {
            public static IEnumerable<object[]> WillDoGeneralSearchTestSource()
            {
                var urls = new[]
                {
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

                return urls.Select(x => new[] { x });
            }

            [Fact]
            public async Task WillFindMatches()
            {
                var url = "https://pp.userapi.com/c639830/v639830431/11db4/peMZxfCdiko.jpg";

                var api = GetIqdbClient();

                var result = await api.SearchUrl(url);

                Assert.NotNull(result);
                Assert.True(result.IsFound);

                Assert.NotNull(result.Matches);
                Assert.Equal(16, result.Matches.Count);
                Assert.Equal(1, result.Matches.Count(match => match.MatchType == MatchType.Additional));
                Assert.Equal(1, result.Matches.Count(match => match.MatchType == MatchType.Best));
                Assert.Equal(14, result.Matches.Count(match => match.MatchType == MatchType.Other));

                AssertSearchStats(result);

                Assert.NotNull(result.YourImage);
                Assert.Equal("peMZxfCdiko.jpg", result.YourImage.Name);
                Assert.False(String.IsNullOrWhiteSpace(result.YourImage.PreviewUrl));
                Assert.Equal("13 KB", result.YourImage.Size);

                AssertMatches(result);
            }

            [Fact]
            public async Task WillNotFindMatches()
            {
                var url = "https://pp.userapi.com/c626224/v626224431/6291e/z3mBzT9q104.jpg";
                var api = GetIqdbClient();

                var result = await api.SearchUrl(url);

                Assert.NotNull(result);
                Assert.False(result.IsFound);

                Assert.NotNull(result.Matches);
                Assert.Equal(0, result.Matches.Count(match => match.MatchType == MatchType.Additional));
                Assert.Equal(0, result.Matches.Count(match => match.MatchType == MatchType.Best));

                AssertSearchStats(result);

                AssertYourImage(result);

                AssertMatches(result);
            }

            [Theory]
            [MemberData(nameof(WillDoGeneralSearchTestSource))]
            public async Task WillDoGeneralSearch(string url)
            {
                var api = GetIqdbClient();

                var result = await api.SearchUrl(url);

                Assert.NotNull(result);
                Assert.NotNull(result.Matches);

                if (result.IsFound)
                {
                    Assert.True(result.Matches.Count(match => match.MatchType == MatchType.Other) >= 0);
                    Assert.True(result.Matches.Count(match => match.MatchType == MatchType.Additional) >= 0);
                    Assert.Equal(1, result.Matches.Count(match => match.MatchType == MatchType.Best));
                }
                else
                {
                    Assert.Equal(0, result.Matches.Count(match => match.MatchType == MatchType.Additional));
                    Assert.Equal(0, result.Matches.Count(match => match.MatchType == MatchType.Best));
                }

                AssertSearchStats(result);

                AssertYourImage(result);

                AssertMatches(result);
            }

            [Fact]
            public async Task WillThrowExceptions()
            {
                var api = GetIqdbClient();

                await Assert.ThrowsAsync<NotImageException>(() =>
                    api.SearchUrl("https://yande.re/favicon.ico"));

                await Assert.ThrowsAsync<NotImageException>(() =>
                    api.SearchUrl("https://iqdb.org/default.css"));

                await Assert.ThrowsAsync<ArgumentException>(() =>
                    api.SearchUrl(""));

                await Assert.ThrowsAsync<ArgumentException>(() =>
                    api.SearchUrl("  "));

                await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    api.SearchUrl(null));


                // http failed
                await Assert.ThrowsAsync<HttpRequestFailed>(() =>
                    api.SearchUrl("asdfasgsdf"));

                // size
                await Assert.ThrowsAsync<ImageTooLagreException>(() =>
                    api.SearchUrl("https://files.yande.re/image/8f8c1f35e4ca613487dda4ea2d282077/yande.re%20377441%20armor%20bodysuit%20gun%20heroes_of_the_storm%20mecha%20monster%20overwatch%20sword%20tattoo%20tracer%20weapon%20zarya.jpg"));

                // resolution
                await Assert.ThrowsAsync<ImageTooLagreException>(() =>
                    api.SearchUrl("https://files.yande.re/image/cd73e77b015a257fa807afdc3043cbc0/yande.re%20277274%20aoki_hagane_no_arpeggio%20ass%20bikini%20morita_kazuaki%20panty_pull%20stick_poster%20swimsuits%20takao_%28aoki_hagane_no_arpeggio%29.jpg"));

            }

            [Fact]
            public async Task WillPauseOnSeveralRequests()
            {
                async Task<DateTimeOffset> GetImage(string url)
                {
                    var api = GetIqdbClient();
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

                var tasks = urls.Select(GetImage).ToArray();


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

                        Assert.NotEqual(holes.ElementAt(i), holes.ElementAt(j));
                    }
                }
            }
        }

        public class TheSearchFileMethod : IqdbApiRealTests
        {
            [Theory]
            [InlineData("Resources/9cc122fe5884a090d1dfe6832b8ed19f.jpg")]
            public async Task WillFindMatches(string filename)
            {
                IIqdbClient api = new IqdbClient();
                SearchResult result;

                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    result = await api.SearchFile(fs);
                }

                Assert.NotNull(result);
                Assert.True(result.IsFound);

                Assert.NotNull(result.Matches);
                Assert.True(result.Matches.Count(match => match.MatchType == MatchType.Other) > 0);
                Assert.True(result.Matches.Count(match => match.MatchType == MatchType.Additional) > 1);
                Assert.True(result.Matches.Count(match => match.MatchType == MatchType.Best) == 1);

                AssertSearchStats(result);

                Assert.NotNull(result.YourImage);
                Assert.False(String.IsNullOrWhiteSpace(result.YourImage.Name));
                Assert.False(String.IsNullOrWhiteSpace(result.YourImage.PreviewUrl));

                foreach (var match in result.Matches)
                {
                    Assert.True(match.Similarity > 0);
                    Assert.NotEqual(default(Resolution), match.Resolution);

                    Assert.False(String.IsNullOrWhiteSpace(match.PreviewUrl));
                    Assert.False(String.IsNullOrWhiteSpace(match.Url));
                }
            }

            [Fact]
            public async Task WillThrowExceptions()
            {
                IIqdbClient api = new IqdbClient();


                using (var fs = new FileStream("Resources/favicon.ico", FileMode.Open))
                {
                    await Assert.ThrowsAsync<InvalidFileFormatException>(() =>
                        api.SearchFile(fs));


                    await Assert.ThrowsAsync<ArgumentNullException>(() =>
                        api.SearchFile(null));
                }

                using (var fs = new FileStream("Resources/large.jpg", FileMode.Open))
                {
                    await Assert.ThrowsAsync<ImageTooLagreException>(() =>
                        api.SearchFile(fs));
                }
            }
        }

        private static void AssertMatches(SearchResult result)
        {
            foreach (var match in result.Matches)
            {
                Assert.True(match.Similarity > 0);

                Assert.False(String.IsNullOrWhiteSpace(match.PreviewUrl));
                Assert.False(String.IsNullOrWhiteSpace(match.Url));
            }
        }

        private static void AssertYourImage(SearchResult result)
        {
            Assert.NotNull(result.YourImage);
            Assert.False(String.IsNullOrWhiteSpace(result.YourImage.Name));
            Assert.False(String.IsNullOrWhiteSpace(result.YourImage.PreviewUrl));
            Assert.False(String.IsNullOrWhiteSpace(result.YourImage.Size));
        }

        private static void AssertSearchStats(SearchResult result)
        {
            Assert.True(result.SearchedImagesCount > 0);
            Assert.True(result.SearchedInSeconds > 0);
        }

        private static IIqdbClient GetIqdbClient() => new IqdbClient(GetHttpHandlerMock());

        private static MockHttpMessageHandler GetHttpHandlerMock()
        {
            var mockHttp = new MockHttpMessageHandler();

            foreach (var urlRespone in IqdbHttpResponsesMock.UrlResponses)
            {
                mockHttp.When(urlRespone.Key)
                    .Respond(HttpStatusCode.OK, new StringContent(urlRespone.Value));
            }

            return mockHttp;
        }
    }
}