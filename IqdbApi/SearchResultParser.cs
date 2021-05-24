using AngleSharp.Dom;
using IqdbApi.Enums;
using IqdbApi.Exceptions;
using IqdbApi.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using Match = IqdbApi.Models.Match;

namespace IqdbApi
{
    internal class SearchResultParser
    {
        private readonly Regex _searchedStatsRegex = new Regex("[0-9,.]{2,}");
        private readonly Regex _scoreRegex = new Regex("Score:\\s([\\d\\.]+)");
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

        public SearchResult ParseResult(string html)
        {
            var parser = new HtmlParser();
            var rootElement = parser.ParseDocument(html).DocumentElement;
            
            ThrowIfIqdbError(rootElement);

            try
            {
                var searchStats = GetSearchStats(rootElement);
                var matches = GetMatches(rootElement);

                var searchResult = new SearchResult
                {
                    SearchedInSeconds = searchStats.SearchInSeconds,
                    SearchedImagesCount = searchStats.SearchedImagesCount,
                    YourImage = matches.YourImage,
                    Matches = ImmutableList.Create(matches.Matches.ToArray())
                };

                return searchResult;
            }
            catch (Exception e)
            {
                throw new FormatException("Html format is not recognizable, unable to parse some values.", e);
            }
        }

        private void ThrowIfIqdbError(IElement documentNode)
        {
            var errorString = documentNode.QuerySelector(".err")?.TextContent;

            if (String.IsNullOrWhiteSpace(errorString))
            {
                return;
            }

            if (errorString.Contains("too large"))
            {
                throw new ImageTooLargeException(innerException: new Exception(errorString));
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

        private (List<Match> Matches, YourImage YourImage) GetMatches(IElement documentNode)
        {
            var mainResults = documentNode.QuerySelectorAll("#pages > div").ToList();

            var yourImageElement = mainResults.First();
            var yourImage = GetYourImage(yourImageElement, documentNode);
            mainResults.Remove(yourImageElement);

            var mainMatches = GetMainMatches(mainResults);

            var otherMatches = GetOtherMatches(documentNode);
            
            return (mainMatches.Union(otherMatches).ToList(), yourImage);
        }

        private IEnumerable<Match> GetMainMatches(IEnumerable<IElement> matchElements) 
            => matchElements.Select(GetMatch).Where(x => x != null);

        private IEnumerable<Match> GetOtherMatches(IElement documentNode)
            => documentNode.QuerySelectorAll("#more1 > div.pages > div").Select(GetOtherMatch);

        private Match GetOtherMatch(IElement match)
        {
            var newMatch = new Match();

            newMatch.MatchType = MatchType.Other;
            newMatch.Url = EnsureScheme(match.QuerySelector("tr:nth-child(1) > td > a")?.Attributes["href"]?.Value);
            newMatch.PreviewUrl = match.QuerySelector("tr:nth-child(1) > td > a > img")?.Attributes["src"]?.Value;

            var alt = match.QuerySelector("tr:nth-child(1) > td > a > img")?.Attributes["alt"]?.Value;
            newMatch.Score = GetScore(alt);
            newMatch.Tags = GetTags(alt);

            newMatch.Source = GetSource(match.QuerySelector("tr:nth-child(2) > td").ChildNodes[1].TextContent);

            var resStrings = match.QuerySelector("tr:nth-child(3) > td")?.TextContent?.Split(' ');
            if (resStrings?.Length == 2)
            {
                newMatch.Resolution = GetResolution(resStrings.First()).Value;
            }

            newMatch.Rating = GetRating(resStrings.Last());
            

            newMatch.Similarity = GetSimilarity(match.QuerySelector("tr:nth-child(4) > td")?.TextContent);

            return newMatch;
        }

        private Match GetMatch(IElement match)
        {
            var newMatch = new Match();

            var matchTypeText = match.QuerySelector("tr:nth-child(1) > th")?.TextContent;
            MatchType? matchType = GetMatchType(matchTypeText);
            if (matchType == null)
            {
                return null;
            }

            newMatch.MatchType = matchType.Value;
            newMatch.Url = EnsureScheme(match.QuerySelector("tr:nth-child(2) > td > a")?.Attributes["href"]?.Value);
            newMatch.PreviewUrl = match.QuerySelector("tr:nth-child(2) > td > a > img")?.Attributes["src"]?.Value;

            var imageAlt = match.QuerySelector("tr:nth-child(2) > td > a > img")?.Attributes["alt"]?.Value;
            newMatch.Score = GetScore(imageAlt);
            newMatch.Tags = GetTags(imageAlt);

            newMatch.Source = GetSource(match.QuerySelector("tr:nth-child(3) > td").ChildNodes[1].TextContent);

            var resStrings = match.QuerySelector("tr:nth-child(4) > td")?.TextContent?.Split(' ');
            if (resStrings != null)
            {
                if (resStrings.Length == 2)
                {
                    newMatch.Resolution = GetResolution(resStrings.First()).Value;
                }
                newMatch.Rating = GetRating(resStrings.Last());
            }
            newMatch.Similarity = GetSimilarity(match.QuerySelector("tr:nth-child(5) > td")?.TextContent);

            return newMatch;
        }

        private static string EnsureScheme(string value)
        {
            if (value?.StartsWith("//") == true)
            {
                return "https:" + value;
            }

            return value;
        }

        private static byte GetSimilarity(string innerText)
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
                case "[Unrated]":
                    return Rating.Unrated;
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

            var score = scoreMatch.Groups[1]?.Captures[0]?.ToString();

            if (score == null 
                || !Byte.TryParse(score, NumberStyles.Any, CultureInfo.InvariantCulture, out byte scoreByte))
            {
                return null;
            }

            return scoreByte;
        }

        private MatchType? GetMatchType(string matchTypeText)
        {
            switch (matchTypeText)
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
                    throw new FormatException($"Match type {matchTypeText} is not supported.");
            }
        }

        private YourImage GetYourImage(IElement element, IElement document)
        {
            var yourImage = new YourImage();

            yourImage.PreviewUrl = element.QuerySelector("tr:nth-child(2) > td > img")?.Attributes["src"]?.Value;
            yourImage.Name = element.QuerySelector("tr:nth-child(3) > td > span")?.Attributes["title"]?.Value;

            var resText = element.QuerySelector("tr:nth-child(4) > td")?.TextContent;
            yourImage.Resolution = GetResolution(resText);

            yourImage.Size = document.QuerySelector("body > div.notice")?.TextContent?.Split('(').Last().Split(')').First();

            return yourImage;
        }

        private (double SearchInSeconds, int SearchedImagesCount) GetSearchStats(IElement rootElement)
        {
            var matches = GetSearchStatsMatches(rootElement);

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

        private MatchCollection GetSearchStatsMatches(IElement rootElement)
        {
            var searchedStatsText = rootElement
                                        .QuerySelector("body > p")
                                        ?.TextContent;

            if (String.IsNullOrWhiteSpace(searchedStatsText))
            {
                throw new FormatException("Can't find search stats info.");
            }

            var matches = _searchedStatsRegex.Matches(searchedStatsText);
            return matches;
        }
        
        private SearchMoreInfo ParseSearchMoreInfo(IElement rootElement)
        {
            var searchMoreNode = rootElement.QuerySelector("#yetmore");
            var serachMoreLink = searchMoreNode?.Attributes["href"]?.Value;

            if (String.IsNullOrWhiteSpace(serachMoreLink))
            {
                throw new FormatException("Can't find search more info.");
            }

            var args = serachMoreLink.Split('&').ToDictionary(x => x.Split('=').First(), y => y.Split('=').Last());

            var searchMoreInfo = new SearchMoreInfo
            {
                Original = args["org"],
                Thumb = args["thu"]
            };
            
            return searchMoreInfo;
        }
    }
}
