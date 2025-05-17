using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using VideoScripter.Features.Videos.Models;

namespace VideoScripter.Features.Videos;

public partial class ProjectVideos
{
    [Parameter] public Guid ProjectId { get; set; }
    [Parameter] public List<VideoSearchResult> Videos { get; set; } = new();
    [Parameter] public EventCallback<string> OnVideoRemoved { get; set; }

    [Inject] private VideoHandler VideoHandler { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private bool showRemoveConfirm = false;
    private VideoSearchResult? videoToRemove;
    private bool isRemoving = false;

    private void ConfirmRemoveVideo(VideoSearchResult video)
    {
        videoToRemove = video;
        showRemoveConfirm = true;
    }

    private void CloseRemoveConfirm()
    {
        showRemoveConfirm = false;
        videoToRemove = null;
    }

    private async Task RemoveVideo()
    {
        if (videoToRemove == null || isRemoving)
            return;

        isRemoving = true;

        try
        {
            var videoId = videoToRemove.Id;
            var success = await VideoHandler.RemoveVideoFromProjectAsync(ProjectId, videoId, "");

            if (success)
            {
                await OnVideoRemoved.InvokeAsync(videoId);
                CloseRemoveConfirm();
            }
        }
        catch (Exception)
        {
            // Handle error
        }
        finally
        {
            isRemoving = false;
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
}