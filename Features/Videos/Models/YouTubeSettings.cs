namespace VideoScripter.Features.Videos.Models;

/// <summary>
/// Configuration settings for YouTube API
/// </summary>
public class YouTubeSettings
{
    public const string SectionName = "YouTube";

    /// <summary>
    /// YouTube API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}