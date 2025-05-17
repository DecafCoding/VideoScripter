
using VideoScripter.Features.Videos.Models;

namespace VideoScripter.Features.Videos.Services;

/// <summary>
/// Interface for YouTube API operations
/// </summary>
public interface IYouTubeApiService
{
    /// <summary>
    /// Searches for YouTube videos based on a search term
    /// </summary>
    Task<List<VideoSearchResult>> SearchVideosAsync(string searchTerm, int maxResults = 25);

    /// <summary>
    /// Gets channel information by YouTube channel ID
    /// </summary>
    Task<ChannelInfo?> GetChannelInfoAsync(string channelId);
}