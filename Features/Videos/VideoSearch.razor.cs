using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Timers;
using VideoScripter.Features.Videos.Models;

namespace VideoScripter.Features.Videos;

public partial class VideoSearch : IDisposable
{
    [Parameter] public Guid ProjectId { get; set; }
    [Parameter] public EventCallback<VideoSearchResult> OnVideoAdded { get; set; }
    [Parameter] public IEnumerable<string> ExistingVideoIds { get; set; } = Enumerable.Empty<string>();

    [Inject] private VideoHandler VideoHandler { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    protected string UserId { get; set; } = string.Empty;

    private string searchTerm = string.Empty;
    private List<VideoSearchResult> searchResults = new();
    private HashSet<VideoSearchResult> selectedVideos = new();
    private bool isFirstSearch = true;
    private bool isSearching = false;
    private bool isAddingVideos = false;
    private string? searchErrorMessage;
    private System.Timers.Timer? debounceTimer;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        UserId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Create a timer for debouncing search
        debounceTimer = new System.Timers.Timer(500);
        debounceTimer.Elapsed += async (sender, e) => await DebounceSearch();
        debounceTimer.AutoReset = false;
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            SearchVideos();
        }
    }

    private void OnSearchTermChanged()
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 3)
            return;

        // Reset and start the debounce timer
        debounceTimer?.Stop();
        debounceTimer?.Start();
    }

    private async Task DebounceSearch()
    {
        // Invoke UI update
        await InvokeAsync(async () =>
        {
            await SearchVideos();
            StateHasChanged();
        });
    }

    private async Task SearchVideos()
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || isSearching)
            return;

        isSearching = true;
        isFirstSearch = false;
        searchErrorMessage = null;
        selectedVideos.Clear();

        try
        {
            var request = new VideoSearchRequest
            {
                SearchTerm = searchTerm,
                MaxResults = 25
            };

            var response = await VideoHandler.SearchVideosAsync(request, UserId);
            searchResults = response.Videos;

            // Mark videos that are already in the project
            foreach (var video in searchResults)
            {
                video.IsSelected = false;
            }
        }
        catch (Exception ex)
        {
            searchErrorMessage = $"Error searching videos: {ex.Message}";
            searchResults.Clear();
        }
        finally
        {
            isSearching = false;
        }
    }

    private void SelectVideo(VideoSearchResult video)
    {
        // Don't allow selection if video is already in the project
        if (ExistingVideoIds.Contains(video.Id))
            return;

        // Toggle selection state
        video.IsSelected = !video.IsSelected;

        if (video.IsSelected)
        {
            selectedVideos.Add(video);
        }
        else
        {
            selectedVideos.Remove(video);
        }
    }

    private async Task AddSelectedVideosToProject()
    {
        if (!selectedVideos.Any() || isAddingVideos)
            return;

        isAddingVideos = true;

        try
        {
            var request = new AddVideosToProjectRequest
            {
                ProjectId = ProjectId,
                VideoIds = selectedVideos.Select(v => v.Id).ToList()
            };

            var response = await VideoHandler.AddVideosToProjectAsync(request, UserId);

            if (response.Success)
            {
                // Notify parent component that videos were added
                foreach (var video in selectedVideos)
                {
                    await OnVideoAdded.InvokeAsync(video);
                }

                // Clear selection after successful addition
                foreach (var video in searchResults)
                {
                    video.IsSelected = false;
                }
                selectedVideos.Clear();

                // Scroll to top
                await JSRuntime.InvokeVoidAsync("window.scrollTo", 0, 0);
            }
            else
            {
                searchErrorMessage = response.Message;
            }
        }
        catch (Exception ex)
        {
            searchErrorMessage = $"Error adding videos to project: {ex.Message}";
        }
        finally
        {
            isAddingVideos = false;
        }
    }

    private string FormatCount(ulong count)
    {
        if (count >= 1_000_000_000)
            return $"{count / 1_000_000_000:0.#}B";
        if (count >= 1_000_000)
            return $"{count / 1_000_000:0.#}M";
        if (count >= 1_000)
            return $"{count / 1_000:0.#}K";
        return count.ToString();
    }

    public void Dispose()
    {
        debounceTimer?.Dispose();
    }
}