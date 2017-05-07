using IqdbApi.Enums;
using System.Collections.Immutable;
using System.Linq;

namespace IqdbApi.Models
{
    public class SearchResult
    {
        public int SearchedImagesCount { get; internal set; }

        public double SearchedInSeconds { get; internal set; }

        public bool IsFound => Matches.Any(match => match.MatchType == MatchType.Best);

        public ImmutableList<Match> Matches { get; internal set; }

        public YourImage YourImage { get; internal set; }
    }
}