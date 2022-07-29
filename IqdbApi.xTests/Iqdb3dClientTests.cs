using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IqdbApi.Models;
using Xunit;
using MatchType = IqdbApi.Enums.MatchType;

namespace IqdbApi.xTests
{
    public class Iqdb3dApiRealTests
    {
        [Fact]
        public async Task WillFindMatchesWithFileSearch()
        {
            var api = new Iqdb3dClient();

            SearchResult result;

            await using (var fs = new FileStream("Resources/453ee0211dd3d3ce119dd686142632e6.jpg", FileMode.Open))
            {
                result = await api.SearchFile(fs);
            }

            Assert.NotNull(result);
            Assert.True(result.IsFound);

            Assert.NotNull(result.Matches);
            Assert.True(result.Matches.Count(match => match.MatchType == MatchType.Other) == 14);
            Assert.True(result.Matches.Count(match => match.MatchType == MatchType.Additional) == 1);
            Assert.True(result.Matches.Count(match => match.MatchType == MatchType.Best) == 1);

            Assert.NotNull(result.YourImage);
            Assert.False(string.IsNullOrWhiteSpace(result.YourImage.Name));
            Assert.False(string.IsNullOrWhiteSpace(result.YourImage.PreviewUrl));

            foreach (var match in result.Matches)
            {
                Assert.True(match.Similarity > 0);
                Assert.NotEqual(default, match.Resolution);

                Assert.False(string.IsNullOrWhiteSpace(match.PreviewUrl));
                Assert.False(string.IsNullOrWhiteSpace(match.Url));
            }
        }

        [Fact]
        public async Task WillFindMatchesWithUrlSearch()
        {
            const string url = "https://sun9-25.userapi.com/impg/YnOMtl6hbLQNLFEFHYyLHVZz8ZuCEXk3Rf3Mjg/jgpcUKmkFm4.jpg?size=600x900&quality=96&sign=0b1bc7f8fa2f2355a00a4b526358c429";
            
            var api = new Iqdb3dClient();

            var result = await api.SearchUrl(url);

            Assert.NotNull(result);
            Assert.True(result.IsFound);

            Assert.NotNull(result.Matches);
            Assert.True(result.Matches.Count(match => match.MatchType == MatchType.Other) == 14);
            Assert.True(result.Matches.Count(match => match.MatchType == MatchType.Additional) == 1);
            Assert.True(result.Matches.Count(match => match.MatchType == MatchType.Best) == 1);

            Assert.NotNull(result.YourImage);
            Assert.False(string.IsNullOrWhiteSpace(result.YourImage.Name));
            Assert.False(string.IsNullOrWhiteSpace(result.YourImage.PreviewUrl));

            foreach (var match in result.Matches)
            {
                Assert.True(match.Similarity > 0);
                Assert.NotEqual(default, match.Resolution);

                Assert.False(string.IsNullOrWhiteSpace(match.PreviewUrl));
                Assert.False(string.IsNullOrWhiteSpace(match.Url));
            }
        }
    }
}
