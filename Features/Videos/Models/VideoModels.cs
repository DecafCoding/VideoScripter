using System.Text.Json.Serialization;

namespace VideoScripter.Features.Videos.Models;

/// <summary>
/// Represents a video search result from YouTube
/// </summary>
public class VideoSearchResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("thumbnailUrl")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [JsonPropertyName("channelId")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonPropertyName("channelTitle")]
    public string ChannelTitle { get; set; } = string.Empty;

    [JsonPropertyName("publishedAt")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("viewCount")]
    public ulong ViewCount { get; set; }

    [JsonPropertyName("likeCount")]
    public ulong LikeCount { get; set; }

    [JsonPropertyName("commentCount")]
    public ulong CommentCount { get; set; }

    [JsonPropertyName("durationSeconds")]
    public int DurationSeconds { get; set; }

    [JsonPropertyName("formattedDuration")]
    public string FormattedDuration => FormatDuration(DurationSeconds);

    [JsonPropertyName("isSelected")]
    public bool IsSelected { get; set; }

    /// <summary>
    /// Formats duration in seconds to a human-readable format (HH:MM:SS or MM:SS)
    /// </summary>
    private static string FormatDuration(int seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        return timeSpan.Hours > 0
            ? $"{timeSpan.Hours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
            : $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}";
    }
}

/// <summary>
/// Represents channel information from YouTube
/// </summary>
public class ChannelInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("thumbnailUrl")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [JsonPropertyName("subscriberCount")]
    public ulong? SubscriberCount { get; set; }

    [JsonPropertyName("videoCount")]
    public ulong? VideoCount { get; set; }

    [JsonPropertyName("publishedAt")]
    public DateTime PublishedAt { get; set; }
}

/// <summary>
/// Request model for video search
/// </summary>
public class VideoSearchRequest
{
    [JsonPropertyName("searchTerm")]
    public string SearchTerm { get; set; } = string.Empty;

    [JsonPropertyName("maxResults")]
    public int MaxResults { get; set; } = 25;
}

/// <summary>
/// Response model for video search
/// </summary>
public class VideoSearchResponse
{
    [JsonPropertyName("videos")]
    public List<VideoSearchResult> Videos { get; set; } = new();

    [JsonPropertyName("totalResults")]
    public int TotalResults => Videos.Count;
}

/// <summary>
/// Request model for adding videos to a project
/// </summary>
public class AddVideosToProjectRequest
{
    [JsonPropertyName("projectId")]
    public Guid ProjectId { get; set; }

    [JsonPropertyName("videoIds")]
    public List<string> VideoIds { get; set; } = new();
}

/// <summary>
/// Response model for adding videos to a project
/// </summary>
public class AddVideosToProjectResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("videoCount")]
    public int VideoCount { get; set; }
}