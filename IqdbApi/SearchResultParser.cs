using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using IqdbApi.Enums;
using IqdbApi.Exceptions;
using IqdbApi.Models;
using Match = IqdbApi.Models.Match;

namespace IqdbApi
{
    internal class SearchResultParser
    {
        private readonly Regex _searchedStatsRegex = new Regex("[0-9,.]{2,}");
        private readonly Regex _scoreRegex = new Regex("Score: (\\w+) ");
        private readonly Dictionary<string, Source> _sourceMapper = new Dictionary<string, Source>
            {
                {"Danbooru ", Source.Danbooru},
                {"Anime-Pictures", Source.AnimePictures},
                {"e-shuushuu", Source.Eshuushuu},
                {"Gelbooru", Source.Gelbooru},
                {"Konachan", Source.Konachan},
                {"Sankaku Channel", Source.SankakuChannel},
                {"The Anime Gallery", Source.TheAnimeGallery},
                {"yande.re", Source.Yandere},
                {"Zerochan", Source.Zerochan}

            };

        public SearchResult Parse(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            CheckOnErrors(doc);

            try
            {
                var searchStats = GetSearchStats(doc);
                var matches = GetMatches(doc);

                var searchResult = new SearchResult
                {
                    SearchedInSeconds = searchStats.SearchInSeconds,
                    SearchedImagesCount = searchStats.SearchedImagesCount,
                    YourImage = matches.YourImage,
                    Matches = matches.Matches
                };

                return searchResult;
            }
            catch (Exception e)
            {
                throw new FormatException("Html format is not recognizable, unable to parse some values.", e);
            }
        }

        private void CheckOnErrors(HtmlDocument doc)
        {
            var errorString = doc.DocumentNode.SelectSingleNode("//*[@class=\"err\"]/text()[1]")?.InnerText;

            if (String.IsNullOrWhiteSpace(errorString))
            {
                return;
            }

            if (errorString.Contains("too large"))
            {
                throw new ImageTooLagreException(innerException: new Exception(errorString));
            }

            if (errorString.Contains("HTTP request failed"))
            {
                throw new HttpRequestFailed(innerException: new Exception(errorString));
            }

            if (errorString.Contains("Not an image"))
            {
                throw new NotImageException(innerException: new Exception(errorString));
            }

            throw new InvalidFileFormatException("Unrecignized exception", new Exception(errorString));
        }

        private (ImmutableList<Match> Matches, YourImage YourImage) GetMatches(HtmlDocument html)
        {
            var result = new List<Match>();

            var mainResults = html.DocumentNode.SelectNodes("//*[@id=\"pages\"]/div").ToList();
            var otherResults = html.DocumentNode.SelectNodes("//*[@id=\"more1\"]/div[2]/div").ToList();

            YourImage yourImage = null;
            bool isFirst = true;
            foreach (var match in mainResults)
            {
                if (isFirst)
                {
                    yourImage = GetYourImage(match);

                    isFirst = false;
                    continue;
                }

                var newMatch = GetMatch(match);

                if (newMatch == null)
                {
                    continue;
                }

                result.Add(newMatch);
            }

            foreach (var match in otherResults)
            {

                Match newMatch = GetOtherMatch(match);

                result.Add(newMatch);
            }

            return (ImmutableList.Create(result.ToArray()), yourImage);
        }

        private Match GetOtherMatch(HtmlNode match)
        {
            var newMatch = new Match();

            newMatch.MatchType = MatchType.Other;
            newMatch.Url = match.SelectSingleNode("table/tr[1]/td/a")?.Attributes["href"]?.Value;
            newMatch.PreviewUrl = match.SelectSingleNode("table/tr[1]/td/a/img")?.Attributes["src"]?.Value;

            var alt = match.SelectSingleNode("table/tr[1]/td/a/img")?.Attributes["alt"]?.Value;
            newMatch.Score = GetScore(alt);
            newMatch.Tags = GetTags(alt);

            newMatch.Source = GetSource(match.SelectSingleNode("table/tr[2]/td/text()").InnerText);

            var resStrings = match.SelectSingleNode("table/tr[3]/td")?.InnerText?.Split(' ');
            newMatch.Resolution = GetResolution(resStrings.First()).Value;
            newMatch.Rating = GetRating(resStrings.Last());

            newMatch.Similarity = GetSimilarity(match.SelectSingleNode("table/tr[4]/td")?.InnerText);

            return newMatch;
        }

        private Match GetMatch(HtmlNode match)
        {
            var newMatch = new Match();

            MatchType? matchType = GetMatchType(match.SelectSingleNode("table/tr[1]/th")?.InnerText);
            if (matchType == null)
            {
                return null;
            }

            newMatch.MatchType = matchType.Value;
            newMatch.Url = match.SelectSingleNode("table/tr[2]/td/a")?.Attributes["href"]?.Value;
            newMatch.PreviewUrl = match.SelectSingleNode("table/tr[2]/td/a/img")?.Attributes["src"]?.Value;

            var alt = match.SelectSingleNode("table/tr[2]/td/a/img")?.Attributes["alt"]?.Value;
            newMatch.Score = GetScore(alt);
            newMatch.Tags = GetTags(alt);

            newMatch.Source = GetSource(match.SelectSingleNode("table/tr[3]/td/text()").InnerText);

            var resStrings = match.SelectSingleNode("table/tr[4]/td")?.InnerText?.Split(' ');
            newMatch.Resolution = GetResolution(resStrings.First()).Value;
            newMatch.Rating = GetRating(resStrings.Last());

            newMatch.Similarity = GetSimilarity(match.SelectSingleNode("table/tr[5]/td")?.InnerText);

            return newMatch;
        }

        private byte GetSimilarity(string innerText)
        {
            var simString = innerText.Split('%').First();

            if (!Byte.TryParse(simString, NumberStyles.Any, CultureInfo.InvariantCulture, out byte sim))
            {
                throw new FormatException($"Can't get similarity from {innerText}.");
            }

            return sim;
        }

        private Resolution? GetResolution(string resolutionString)
        {
            if (String.IsNullOrWhiteSpace(resolutionString))
            {
                return null;
            }

            var resText = resolutionString.Split('×');
            return new Resolution
            {
                Width = UInt32.Parse(resText.First()),
                Height = UInt32.Parse(resText.Last())
            };
        }

        private Source GetSource(string innerText)
        {

            if (!_sourceMapper.ContainsKey(innerText))
            {
                throw new FormatException($"Can't get source from {innerText}.");
            }

            return _sourceMapper[innerText];
        }

        private ImmutableList<string> GetTags(string alt)
        {
            var i = alt.IndexOf("Tags: ", StringComparison.Ordinal);

            if (i < 0)
            {
                return ImmutableList<string>.Empty;
            }

            var tags = alt.Substring(i + 6).Split(' ');
            return ImmutableList.Create(tags);
        }

        private Rating GetRating(string rating)
        {
            switch (rating)
            {
                case "[Ero]":
                    return Rating.Questionable;
                case "[Safe]":
                    return Rating.Safe;
                case "[Explicit]":
                    return Rating.Explicit;
                default:
                    throw new FormatException($"Can't get rating from {rating}.");
            }
        }

        private byte? GetScore(string alt)
        {
            var scoreMatch = _scoreRegex.Match(alt);
            if (scoreMatch.Groups.Count < 2)
            {
                return null;
            }

            var score = scoreMatch?.Groups[1]?.Captures[0]?.ToString();

            if (score == null 
                || !Byte.TryParse(score, NumberStyles.Any, CultureInfo.InvariantCulture, out byte scoreByte))
            {
                return null;
            }

            return scoreByte;
        }

        private MatchType? GetMatchType(string innerText)
        {
            switch (innerText)
            {
                case "No relevant matches":
                    return null;
                case "Best match":
                    return MatchType.Best;
                case "Additional match":
                    return MatchType.Additional;
                case "Possible match":
                    return MatchType.Possible;
                default:
                    throw new FormatException($"Match type {innerText} is not supported.");
            }
        }

        private YourImage GetYourImage(HtmlNode match)
        {
            var yourImage = new YourImage();

            yourImage.PreviewUrl = match.SelectSingleNode("table/tr[2]/td/img")?.Attributes["src"]?.Value;
            yourImage.Name = match.SelectSingleNode("table/tr[3]/td/span")?.Attributes["title"]?.Value;

            var resText = match.SelectSingleNode("table/tr[4]/td")?.InnerText;
            yourImage.Resolution = GetResolution(resText);

            yourImage.Size = match.OwnerDocument.DocumentNode.SelectSingleNode("/html/body/div[1]")?.InnerText?.Split('(').Last().Split(')').First();

            return yourImage;
        }

        private (double SearchInSeconds, int SearchedImagesCount) GetSearchStats(HtmlDocument html)
        {
            var matches = GetSearchStatsMatches(html);

            var last = matches[matches.Count - 1].ToString();
            var first = matches[0].ToString();
            

            if (!Double.TryParse(last,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double seconds))
            {
                throw new FormatException($"Can't get double from {last} for search stats.");
            }

            if (!Int32.TryParse(first,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out int imageCount))
            {
                throw new FormatException($"Can't get int from {first} for search stats.");
            }

            return (seconds, imageCount);
        }

        private MatchCollection GetSearchStatsMatches(HtmlDocument html)
        {
            var searchedStatsNode = html.DocumentNode.SelectSingleNode("/html/body/p[1]");
            var searchedStatsText = searchedStatsNode?.InnerText;

            if (String.IsNullOrWhiteSpace(searchedStatsText))
            {
                throw new FormatException("Can't find search stats info.");
            }

            var matches = _searchedStatsRegex.Matches(searchedStatsText);
            return matches;
        }
        
        private SearchMoreInfo ParseSearchMoreInfo(HtmlDocument html)
        {
            var searchMoreNode = html.DocumentNode.SelectSingleNode("//*[@id=\"yetmore\"]/");
            var serachMoreLink = searchMoreNode?.Attributes["href"]?.Value;

            if (String.IsNullOrWhiteSpace(serachMoreLink))
            {
                throw new FormatException("Can't find search more info.");
            }

            var args = serachMoreLink.Split('&').ToDictionary(x => x.Split('=').First(), y => y.Split('=').Last());

            var searchMoreInfo = new SearchMoreInfo();

            searchMoreInfo.Original = args["org"];
            searchMoreInfo.Thumb = args["thu"];

            return searchMoreInfo;

        }
    }
}
