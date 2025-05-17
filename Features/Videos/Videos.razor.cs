using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;

using VideoScripter.Features.Projects.Models;
using VideoScripter.Features.Videos.Models;
using VideoScripter.Features.Projects;

namespace VideoScripter.Features.Videos;

[Authorize]
public partial class Videos
{
    [Parameter] public Guid ProjectId { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private VideoHandler VideoHandler { get; set; } = default!;
    [Inject] private ProjectHandler ProjectHandler { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    private ProjectModel? project;
    private List<VideoSearchResult> projectVideos = new();
    private HashSet<string> projectVideoIds = new();
    private string? statusMessage;
    private bool isLoading = true;
    private string userId = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        await LoadProject();
    }

    protected override async Task OnParametersSetAsync()
    {
        // Reload if ProjectId changes
        if (project == null || project.Id != ProjectId)
        {
            await LoadProject();
        }
    }

    private async Task LoadProject()
    {
        isLoading = true;
        statusMessage = null;

        try
        {
            // Load project details
            project = await ProjectHandler.GetProjectByIdAsync(ProjectId, userId);

            if (project != null)
            {
                // Load project videos
                await LoadProjectVideos();
            }
        }
        catch (Exception ex)
        {
            statusMessage = $"Error loading project: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadProjectVideos()
    {
        try
        {
            projectVideos = await VideoHandler.GetProjectVideosAsync(ProjectId, userId);
            projectVideoIds = new HashSet<string>(projectVideos.Select(v => v.Id));
        }
        catch (Exception ex)
        {
            statusMessage = $"Error loading project videos: {ex.Message}";
        }
    }

    private async Task HandleVideoAdded(VideoSearchResult video)
    {
        try
        {
            // Video was added, refresh the project videos
            await LoadProjectVideos();
            statusMessage = $"Video '{video.Title}' added to project successfully.";
        }
        catch (Exception ex)
        {
            statusMessage = $"Error adding video: {ex.Message}";
        }
    }

    private async Task HandleVideoRemoved(string videoId)
    {
        try
        {
            // Video was removed, refresh the project videos
            await LoadProjectVideos();
            statusMessage = "Video removed from project successfully.";
        }
        catch (Exception ex)
        {
            statusMessage = $"Error removing video: {ex.Message}";
        }
    }
}