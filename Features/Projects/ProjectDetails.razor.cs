using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;

using VideoScripter.Features.Projects.Models;
using Microsoft.JSInterop;

namespace VideoScripter.Features.Projects;

[Authorize]
public partial class ProjectDetails
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ProjectHandler ProjectHandler { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    [Parameter] public Guid ProjectId { get; set; }
    protected string UserId { get; set; } = string.Empty;

    private ProjectModel? project;
    private bool isLoading = true;
    private string? statusMessage;

    // Modal state
    private bool showEditModal;
    private bool showDeleteModal;

    // Form model
    private EditProjectModel editForm = new();

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        UserId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        await LoadProject();
    }

    protected override async Task OnParametersSetAsync()
    {
        // Reload project if ProjectId parameter changes
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
            project = await ProjectHandler.GetProjectByIdAsync(ProjectId, UserId);
        }
        catch (Exception ex)
        {
            statusMessage = "Error loading project. Please try again.";
            // Log the exception in a real application
        }
        finally
        {
            isLoading = false;
        }
    }

    private void EditProject()
    {
        if (project == null) return;

        editForm = new EditProjectModel
        {
            Id = project.Id,
            Name = project.Name,
            Topic = project.Topic
        };
        showEditModal = true;
        statusMessage = null;
    }

    private void CloseEditModal()
    {
        showEditModal = false;
        editForm = new EditProjectModel();
    }

    private async Task SaveProject()
    {
        if (isLoading) return;

        isLoading = true;
        statusMessage = null;

        try
        {
            var result = await ProjectHandler.UpdateProjectAsync(editForm, UserId);

            if (result != null)
            {
                project = result; // Update the current project with the new data
                statusMessage = "Project updated successfully!";
                CloseEditModal();
            }
            else
            {
                statusMessage = "Error: Project not found or you don't have permission to edit it.";
            }
        }
        catch (Exception ex)
        {
            statusMessage = "Error updating project. Please try again.";
            // Log the exception in a real application
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ConfirmDeleteProject()
    {
        showDeleteModal = true;
        statusMessage = null;
    }

    private void CloseDeleteModal()
    {
        showDeleteModal = false;
    }

    private async Task DeleteProject()
    {
        if (isLoading) return;

        isLoading = true;
        statusMessage = null;

        try
        {
            var success = await ProjectHandler.DeleteProjectAsync(ProjectId, UserId);

            if (success)
            {
                // Navigate back to projects list after successful deletion
                Navigation.NavigateTo("/projects");
            }
            else
            {
                statusMessage = "Error: Project not found or you don't have permission to delete it.";
                CloseDeleteModal();
            }
        }
        catch (Exception ex)
        {
            statusMessage = "Error deleting project. Please try again.";
            CloseDeleteModal();
            // Log the exception in a real application
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ExportProject()
    {
        // Implement export functionality here
        statusMessage = "Export functionality is not yet implemented.";
    }
}