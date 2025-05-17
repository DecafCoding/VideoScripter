using VideoScripter.Features.Videos;
using VideoScripter.Features.Videos.Models;
using VideoScripter.Features.Videos.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class VideoServiceExtensions
{
    /// <summary>
    /// Adds video-related services to the service collection
    /// </summary>
    public static IServiceCollection AddVideoServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register YouTube API settings
        services.Configure<YouTubeSettings>(
            configuration.GetSection(YouTubeSettings.SectionName));

        // Register services
        services.AddScoped<IYouTubeApiService, YouTubeApiService>();
        services.AddScoped<VideoHandler>();

        return services;
    }
}