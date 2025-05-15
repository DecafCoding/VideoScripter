using Microsoft.AspNetCore.Components;
using VideoScripter.Features.Projects.Models;

namespace VideoScripter.Features.Projects;

public partial class ProjectDetails
{
    [Parameter] public Guid ProjectId { get; set; }
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;

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
            var user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            project = await ProjectHandler.GetProjectByIdAsync(ProjectId, user.Id);
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
            var user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            var result = await ProjectHandler.UpdateProjectAsync(editForm, user.Id);

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
            var user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            var success = await ProjectHandler.DeleteProjectAsync(ProjectId, user.Id);

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