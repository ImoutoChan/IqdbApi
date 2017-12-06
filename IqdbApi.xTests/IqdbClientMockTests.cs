using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using IqdbApi.Enums;
using IqdbApi.Exceptions;
using IqdbApi.Models;
using IqdbApi.xTests.Helpers;
using RichardSzalay.MockHttp;
using RichardSzalay.MockHttp.Matchers;
using Xunit;

namespace IqdbApi.xTests.IqdbApiTestContainer
{
    public class IqdbApiMockTests
    {
        public class TheSearchUrlMethod : IqdbApiMockTests
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
                    "https://files.yande.re/sample/5aa94d7e5e8a7209f889459ef9f2ab9e/"
                        + "yande.re 392121 sample amatsukaze_(kancolle)"
                        + " kantai_collection pantsu rensouhou-kun see_through seifuku"
                        + " snow_is_ stockings string_panties thighhighs wet.jpg",
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
                await Assert.ThrowsAsync<ImageTooLargeException>(() =>
                    api.SearchUrl("https://files.yande.re/image/8f8c1f35e4ca613487dda4ea2d282077/"
                                      + "yande.re%20377441%20armor%20bodysuit%20gun%20heroes_of_the_storm%20mecha%20monster"
                                      + "%20overwatch%20sword%20tattoo%20tracer%20weapon%20zarya.jpg"));

                // resolution
                await Assert.ThrowsAsync<ImageTooLargeException>(() =>
                    api.SearchUrl("https://files.yande.re/image/cd73e77b015a257fa807afdc3043cbc0/"
                                      + "yande.re%20277274%20aoki_hagane_no_arpeggio%20ass%20bikini%20morita_kazuaki"
                                      + "%20panty_pull%20stick_poster%20swimsuits%20takao_%28aoki_hagane_no_arpeggio%29.jpg"));

            }

            [Theory]
            [InlineData(10)]
            [InlineData(50)]
            [InlineData(100)]
            [InlineData(250)]
            [InlineData(500)]
            [InlineData(1000)]
            public async Task WillPauseOnSeveralRequests(int delay)
            {
                var requestTimes = new List<DateTimeOffset>();
                var testDelay = delay; // 1 * 1000;
                var api = GetIqdbClient(testDelay, () => requestTimes.Add(DateTimeOffset.Now));

                async Task GetImage(string url) => await api.SearchUrl(url);

                var urls = new[]
                {
                    "https://pp.userapi.com/c639830/v639830431/102ad/KknV_pMoh1A.jpg",
                    "https://pp.userapi.com/c626224/v626224431/5dc6b/QqJkFerqHso.jpg",
                    "https://pp.userapi.com/c626224/v626224431/5decf/VvTpyU8A0Zc.jpg",
                    "https://pp.userapi.com/c626224/v626224431/5f1bf/v4xwKIUIaaM.jpg",
                    "https://pp.userapi.com/c636425/v636425431/4d13c/R20-IOXNFds.jpg"
                };
                
                await Task.WhenAll(urls.Select(GetImage));


                var differences = new List<double>();
                for (int i = 0; i < requestTimes.Count - 1; i++)
                {
                    differences.Add((requestTimes[i + 1] - requestTimes[i]).TotalMilliseconds);
                }

                differences.Should().OnlyContain(x => x >= testDelay - 50);
            }
        }

        public class TheSearchFileMethod : IqdbApiMockTests
        {
            [Theory]
            [InlineData("Resources/9cc122fe5884a090d1dfe6832b8ed19f.jpg")]
            public async Task WillFindMatches(string filename)
            {
                IIqdbClient api = GetIqdbClient();
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

            [Fact(Skip = "don't work unless run exclusively")]
            public async Task WillThrowExceptions()
            {
                IIqdbClient api = GetIqdbClient();

                using (var fs = new FileStream("Resources/favicon.ico", FileMode.Open))
                {
                    await Assert.ThrowsAsync<InvalidFileFormatException>(() =>
                        api.SearchFile(fs));
                }

                await Assert.ThrowsAsync<ArgumentNullException>(() => api.SearchFile(null));
                
                using (var fs = new FileStream("Resources/large.jpg", FileMode.Open))
                {
                    await Assert.ThrowsAsync<ImageTooLargeException>(() =>
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

        private static IIqdbClient GetIqdbClient(int delay = 0, Action actionOnRequest = null) 
            => new IqdbClient(GetHttpHandlerMock(actionOnRequest), delay);

        private static HttpMessageHandler GetHttpHandlerMock(Action actionOnRequest = null)
        {
            var mockHttp = new ActionOnHttpMessageHandler();

            mockHttp.SetAction(actionOnRequest);

            foreach (var urlRespone in IqdbHttpResponsesMock.GetResponses)
            {
                mockHttp.When(urlRespone.Key)
                    .Respond(HttpStatusCode.OK, new StringContent(urlRespone.Value));
            }

            foreach (var urlRespone in IqdbHttpResponsesMock.PostResponses)
            {
                mockHttp
                    .When(HttpMethod.Post, "https://iqdb.org")
                    .With(new CustomMatcher(httpRequestMessage 
                                                => CompareContent(httpRequestMessage.Content, urlRespone.GetLength())))
                    .Respond(HttpStatusCode.OK, new StringContent(urlRespone.Response));
            }

            return mockHttp;
        }

        private static bool CompareContent(HttpContent httpClientContent, long fileLength)
        {
            var httpContent = httpClientContent as MultipartFormDataContent;

            var fileContent = httpContent?.OfType<StreamContent>()?.LastOrDefault();

            if (fileContent == null)
                return false;

            var result = fileContent.Headers.ContentLength == fileLength;
            return result;
        }
    }
}