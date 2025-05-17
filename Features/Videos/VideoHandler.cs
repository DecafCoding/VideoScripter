using Microsoft.EntityFrameworkCore;
using VideoScripter.Data;
using VideoScripter.Data.Entities;
using VideoScripter.Features.Videos.Models;
using VideoScripter.Features.Videos.Services;

namespace VideoScripter.Features.Videos;

/// <summary>
/// Handler for video-related operations
/// </summary>
public class VideoHandler
{
    private readonly ApplicationDbContext _context;
    private readonly IYouTubeApiService _youTubeApiService;
    private readonly ILogger<VideoHandler> _logger;

    public VideoHandler(
        ApplicationDbContext context,
        IYouTubeApiService youTubeApiService,
        ILogger<VideoHandler> logger)
    {
        _context = context;
        _youTubeApiService = youTubeApiService;
        _logger = logger;
    }

    /// <summary>
    /// Searches for videos on YouTube based on search term
    /// </summary>
    public async Task<VideoSearchResponse> SearchVideosAsync(VideoSearchRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Searching for videos with term: {SearchTerm}", request.SearchTerm);

            var videos = await _youTubeApiService.SearchVideosAsync(
                request.SearchTerm,
                request.MaxResults);

            return new VideoSearchResponse
            {
                Videos = videos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching videos with term: {SearchTerm}", request.SearchTerm);
            throw;
        }
    }

    /// <summary>
    /// Adds videos to a project
    /// </summary>
    public async Task<AddVideosToProjectResponse> AddVideosToProjectAsync(
        AddVideosToProjectRequest request,
        string userId)
    {
        try
        {
            _logger.LogInformation(
                "Adding {Count} videos to project {ProjectId}",
                request.VideoIds.Count,
                request.ProjectId);

            // Verify the project exists and belongs to the user
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId &&
                                         p.UserId == userId &&
                                         !p.IsDeleted);

            if (project == null)
            {
                return new AddVideosToProjectResponse
                {
                    Success = false,
                    Message = "Project not found or you don't have access to it"
                };
            }

            // Get details for each video from YouTube API
            var videosToAdd = new List<Video>();
            var existingChannels = new Dictionary<string, Guid>();

            foreach (var videoId in request.VideoIds)
            {
                // Check if the video already exists in this project
                var existingVideo = await _context.Videos
                    .AnyAsync(v => v.YTId == videoId &&
                                   v.ProjectId == request.ProjectId &&
                                   !v.IsDeleted);

                if (existingVideo)
                {
                    _logger.LogInformation("Video {VideoId} already exists in project {ProjectId}",
                        videoId, request.ProjectId);
                    continue;
                }

                // Get video details directly through the search (optimized approach)
                var videoSearchResults = await _youTubeApiService.SearchVideosAsync($"id:{videoId}", 1);
                if (!videoSearchResults.Any())
                {
                    _logger.LogWarning("Video with ID {VideoId} not found on YouTube", videoId);
                    continue;
                }

                var videoDetails = videoSearchResults.First();
                var channelId = videoDetails.ChannelId;

                // Check if we already have the channel in our database
                Guid channelDbId;
                if (existingChannels.ContainsKey(channelId))
                {
                    channelDbId = existingChannels[channelId];
                }
                else
                {
                    var existingChannel = await _context.Channels
                        .FirstOrDefaultAsync(c => c.YTId == channelId && !c.IsDeleted);

                    if (existingChannel != null)
                    {
                        channelDbId = existingChannel.Id;
                        existingChannels[channelId] = channelDbId;
                    }
                    else
                    {
                        // Create a new channel if it doesn't exist
                        var channelInfo = await _youTubeApiService.GetChannelInfoAsync(channelId);
                        if (channelInfo == null)
                        {
                            _logger.LogWarning("Channel with ID {ChannelId} not found on YouTube", channelId);
                            continue;
                        }

                        var newChannel = new Channel
                        {
                            Id = Guid.NewGuid(),
                            YTId = channelId,
                            Title = channelInfo.Title,
                            Description = channelInfo.Description,
                            ThumbnailURL = channelInfo.ThumbnailUrl,
                            SubscriberCount = channelInfo.SubscriberCount,
                            VideoCount = channelInfo.VideoCount,
                            PublishedAt = channelInfo.PublishedAt,
                            CreatedBy = userId,
                            LastModifiedBy = userId
                        };

                        _context.Channels.Add(newChannel);
                        channelDbId = newChannel.Id;
                        existingChannels[channelId] = channelDbId;
                    }
                }

                // Create the video entity
                var video = new Video
                {
                    Id = Guid.NewGuid(),
                    ProjectId = request.ProjectId,
                    YTId = videoId,
                    Title = videoDetails.Title,
                    Description = videoDetails.Description,
                    ChannelId = channelDbId,
                    ViewCount = (int)videoDetails.ViewCount,
                    LikeCount = (int)videoDetails.LikeCount,
                    CommentCount = (int)videoDetails.CommentCount,
                    Duration = videoDetails.DurationSeconds,
                    PublishedAt = videoDetails.PublishedAt,
                    CreatedBy = userId,
                    LastModifiedBy = userId
                };

                videosToAdd.Add(video);
            }

            if (videosToAdd.Any())
            {
                await _context.Videos.AddRangeAsync(videosToAdd);
                await _context.SaveChangesAsync();
            }

            return new AddVideosToProjectResponse
            {
                Success = true,
                Message = $"Successfully added {videosToAdd.Count} videos to the project",
                VideoCount = videosToAdd.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding videos to project {ProjectId}", request.ProjectId);
            return new AddVideosToProjectResponse
            {
                Success = false,
                Message = $"Error adding videos: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Gets videos for a specific project
    /// </summary>
    public async Task<List<VideoSearchResult>> GetProjectVideosAsync(Guid projectId, string userId)
    {
        try
        {
            // Verify the project exists and belongs to the user
            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == projectId &&
                               p.UserId == userId &&
                               !p.IsDeleted);

            if (!projectExists)
            {
                _logger.LogWarning("Project {ProjectId} not found or user {UserId} does not have access", projectId, userId);
                return new List<VideoSearchResult>();
            }

            // Get videos from the database
            var videos = await _context.Videos
                .Include(v => v.Channel)
                .Where(v => v.ProjectId == projectId && !v.IsDeleted)
                .Select(v => new VideoSearchResult
                {
                    Id = v.YTId,
                    Title = v.Title,
                    Description = v.Description,
                    ChannelId = v.Channel.YTId,
                    ChannelTitle = v.Channel.Title ?? "Unknown Channel",
                    PublishedAt = v.PublishedAt,
                    ViewCount = (ulong)v.ViewCount,
                    LikeCount = (ulong)v.LikeCount,
                    CommentCount = (ulong)v.CommentCount,
                    DurationSeconds = v.Duration
                })
                .ToListAsync();

            return videos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting videos for project {ProjectId}", projectId);
            throw;
        }
    }

    /// <summary>
    /// Removes a video from a project
    /// </summary>
    public async Task<bool> RemoveVideoFromProjectAsync(Guid projectId, string videoId, string userId)
    {
        try
        {
            // Verify the project exists and belongs to the user
            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == projectId &&
                               p.UserId == userId &&
                               !p.IsDeleted);

            if (!projectExists)
            {
                _logger.LogWarning(
                    "Project {ProjectId} not found or user {UserId} does not have access",
                    projectId, userId);
                return false;
            }

            // Find the video
            var video = await _context.Videos
                .FirstOrDefaultAsync(v => v.ProjectId == projectId &&
                                         v.YTId == videoId &&
                                         !v.IsDeleted);

            if (video == null)
            {
                _logger.LogWarning(
                    "Video {VideoId} not found in project {ProjectId}",
                    videoId, projectId);
                return false;
            }

            // Soft delete the video (mark as deleted)
            video.IsDeleted = true;
            video.LastModifiedBy = userId;
            video.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error removing video {VideoId} from project {ProjectId}",
                videoId, projectId);
            return false;
        }
    }
}