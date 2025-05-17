using System.Text.RegularExpressions;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;
using VideoScripter.Features.Videos.Models;

namespace VideoScripter.Features.Videos.Services;

/// <summary>
/// Service for interacting with the YouTube API
/// </summary>
public class YouTubeApiService : IYouTubeApiService
{
    private readonly YouTubeService _youTubeService;
    private readonly ILogger<YouTubeApiService> _logger;

    public YouTubeApiService(IOptions<YouTubeSettings> settings, ILogger<YouTubeApiService> logger)
    {
        _logger = logger;

        // Initialize YouTube service with API key from configuration
        _youTubeService = new YouTubeService(new BaseClientService.Initializer
        {
            ApiKey = settings.Value.ApiKey,
            ApplicationName = "VideoScripter"
        });
    }

    /// <summary>
    /// Searches for YouTube videos based on a search term
    /// </summary>
    /// <param name="searchTerm">Text to search for</param>
    /// <param name="maxResults">Maximum number of results to return (default: 25)</param>
    /// <returns>List of video search results</returns>
    public async Task<List<VideoSearchResult>> SearchVideosAsync(string searchTerm, int maxResults = 25)
    {
        try
        {
            var searchListRequest = _youTubeService.Search.List("snippet");
            searchListRequest.Q = searchTerm;
            searchListRequest.Type = "video";
            searchListRequest.Order = Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Relevance;
            searchListRequest.MaxResults = maxResults;
            searchListRequest.VideoDuration = Google.Apis.YouTube.v3.SearchResource.ListRequest.VideoDurationEnum.Medium; // Filter for medium-length videos

            var searchResponse = await searchListRequest.ExecuteAsync();

            if (searchResponse.Items == null || !searchResponse.Items.Any())
            {
                return new List<VideoSearchResult>();
            }

            // Extract video IDs for additional details
            var videoIds = searchResponse.Items.Select(item => item.Id.VideoId).ToList();
            var videoDetails = await GetVideoDetailsAsync(videoIds);

            // Map search results to our model
            var results = new List<VideoSearchResult>();
            foreach (var searchResult in searchResponse.Items)
            {
                var details = videoDetails.FirstOrDefault(v => v.Id == searchResult.Id.VideoId);

                var video = new VideoSearchResult
                {
                    Id = searchResult.Id.VideoId,
                    Title = searchResult.Snippet.Title,
                    Description = searchResult.Snippet.Description,
                    ThumbnailUrl = searchResult.Snippet.Thumbnails.Medium?.Url ?? searchResult.Snippet.Thumbnails.Default__?.Url,
                    ChannelId = searchResult.Snippet.ChannelId,
                    ChannelTitle = searchResult.Snippet.ChannelTitle,
                    PublishedAt = DateTime.Parse(searchResult.Snippet.PublishedAtRaw),
                    ViewCount = details?.Statistics?.ViewCount ?? 0,
                    LikeCount = details?.Statistics?.LikeCount ?? 0,
                    CommentCount = details?.Statistics?.CommentCount ?? 0,
                    DurationSeconds = details != null ? ConvertISO8601ToSeconds(details.ContentDetails?.Duration) : 0
                };

                results.Add(video);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching YouTube videos for term: {SearchTerm}", searchTerm);
            throw new YouTubeApiException("Failed to search YouTube videos", ex);
        }
    }

    /// <summary>
    /// Gets detailed information for a list of videos by their IDs
    /// </summary>
    /// <param name="videoIds">List of YouTube video IDs</param>
    /// <returns>List of video detail objects</returns>
    private async Task<IList<Google.Apis.YouTube.v3.Data.Video>> GetVideoDetailsAsync(IList<string> videoIds)
    {
        try
        {
            var videoRequest = _youTubeService.Videos.List("snippet,contentDetails,statistics");
            videoRequest.Id = string.Join(",", videoIds);

            var response = await videoRequest.ExecuteAsync();
            return response.Items ?? new List<Google.Apis.YouTube.v3.Data.Video>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video details for {Count} videos", videoIds.Count);
            return new List<Google.Apis.YouTube.v3.Data.Video>();
        }
    }

    /// <summary>
    /// Gets channel information by YouTube channel ID
    /// </summary>
    /// <param name="channelId">YouTube channel ID</param>
    /// <returns>Channel information object</returns>
    public async Task<ChannelInfo?> GetChannelInfoAsync(string channelId)
    {
        try
        {
            var channelRequest = _youTubeService.Channels.List("snippet,statistics");
            channelRequest.Id = channelId;

            var response = await channelRequest.ExecuteAsync();

            if (response.Items == null || !response.Items.Any())
            {
                return null;
            }

            var channelItem = response.Items.First();

            return new ChannelInfo
            {
                Id = channelItem.Id,
                Title = channelItem.Snippet.Title,
                Description = channelItem.Snippet.Description,
                ThumbnailUrl = channelItem.Snippet.Thumbnails.Medium?.Url ?? channelItem.Snippet.Thumbnails.Default__?.Url,
                SubscriberCount = channelItem.Statistics?.SubscriberCount ?? 0,
                VideoCount = channelItem.Statistics?.VideoCount ?? 0,
                PublishedAt = DateTime.Parse(channelItem.Snippet.PublishedAtRaw)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel info for channel ID: {ChannelId}", channelId);
            throw new YouTubeApiException($"Failed to get channel information for {channelId}", ex);
        }
    }

    /// <summary>
    /// Converts ISO 8601 duration format to seconds
    /// </summary>
    /// <param name="iso8601Duration">Duration in ISO 8601 format (e.g., PT1H30M15S)</param>
    /// <returns>Duration in seconds</returns>
    private static int ConvertISO8601ToSeconds(string? iso8601Duration)
    {
        if (string.IsNullOrEmpty(iso8601Duration))
            return 0;

        var match = Regex.Match(iso8601Duration, @"PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?");

        if (!match.Success)
            return 0;

        int hours = !string.IsNullOrEmpty(match.Groups[1].Value) ? int.Parse(match.Groups[1].Value) : 0;
        int minutes = !string.IsNullOrEmpty(match.Groups[2].Value) ? int.Parse(match.Groups[2].Value) : 0;
        int seconds = !string.IsNullOrEmpty(match.Groups[3].Value) ? int.Parse(match.Groups[3].Value) : 0;

        return hours * 3600 + minutes * 60 + seconds;
    }
}

/// <summary>
/// Custom exception for YouTube API related errors
/// </summary>
public class YouTubeApiException : Exception
{
    public YouTubeApiException(string message) : base(message) { }
    public YouTubeApiException(string message, Exception innerException) : base(message, innerException) { }
}