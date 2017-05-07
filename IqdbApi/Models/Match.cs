using System.Collections.Immutable;
using IqdbApi.Enums;

namespace IqdbApi.Models
{
    public class Match
    {
        public MatchType MatchType { get; internal set; }

        public string Url { get; internal set; }

        public string PreviewUrl { get; internal set; }

        public Rating Rating { get; internal set; }

        public byte? Score { get; internal set; }

        public ImmutableList<string> Tags { get; internal set; }

        public Source Source { get; internal set; }

        public Resolution Resolution { get; internal set; }

        public byte Similarity { get; internal set; }
    }
}