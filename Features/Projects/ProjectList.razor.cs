using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;

using VideoScripter.Features.Projects.Models;
using Microsoft.JSInterop;

namespace VideoScripter.Features.Projects;

[Authorize]
public partial class ProjectList : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ProjectHandler ProjectHandler{ get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected string UserId { get; set; } = string.Empty;

    private List<ProjectModel>? projects;
    private string? statusMessage;
    private bool isLoading;

    // Modal state
    private bool showProjectModal;
    private bool showDeleteModal;
    private bool isEditMode;

    // Form models
    private CreateProjectModel projectForm = new();
    private EditProjectModel editForm = new();
    private ProjectModel? projectToDelete;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        UserId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        await LoadProjects();
    }

    private async Task LoadProjects()
    {
        try
        {
            projects = await ProjectHandler.GetUserProjectsAsync(UserId);
        }
        catch (Exception ex)
        {
            statusMessage = "Error loading projects. Please try again.";
            // Log the exception in a real application
        }
    }

    private void ShowCreateModal()
    {
        projectForm = new CreateProjectModel();
        isEditMode = false;
        showProjectModal = true;
        statusMessage = null;
    }

    private void EditProject(ProjectModel project)
    {
        editForm = new EditProjectModel
        {
            Id = project.Id,
            Name = project.Name,
            Topic = project.Topic
        };
        isEditMode = true;
        showProjectModal = true;
        statusMessage = null;
    }

    private void CloseProjectModal()
    {
        showProjectModal = false;
        projectForm = new CreateProjectModel();
        editForm = new EditProjectModel();
    }

    private async Task SaveProject()
    {
        if (isLoading) return;

        isLoading = true;
        statusMessage = null;

        try
        {
            if (isEditMode)
            {
                var result = await ProjectHandler.UpdateProjectAsync(editForm, UserId);
                if (result != null)
                {
                    statusMessage = "Project updated successfully!";
                    await LoadProjects(); // Refresh the list
                    CloseProjectModal();
                }
                else
                {
                    statusMessage = "Error: Project not found or you don't have permission to edit it.";
                }
            }
            else
            {
                await ProjectHandler.CreateProjectAsync(projectForm, UserId);
                statusMessage = "Project created successfully!";
                await LoadProjects(); // Refresh the list
                CloseProjectModal();
            }
        }
        catch (Exception ex)
        {
            statusMessage = $"Error saving project. Please try again.";
            // Log the exception in a real application
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ConfirmDeleteProject(ProjectModel project)
    {
        projectToDelete = project;
        showDeleteModal = true;
        statusMessage = null;
    }

    private void CloseDeleteModal()
    {
        showDeleteModal = false;
        projectToDelete = null;
    }

    private async Task DeleteProject()
    {
        if (isLoading || projectToDelete == null) return;

        isLoading = true;
        statusMessage = null;

        try
        {
            var success = await ProjectHandler.DeleteProjectAsync(projectToDelete.Id, UserId);

            if (success)
            {
                statusMessage = "Project deleted successfully.";
                await LoadProjects(); // Refresh the list
                CloseDeleteModal();
            }
            else
            {
                statusMessage = "Error: Project not found or you don't have permission to delete it.";
            }
        }
        catch (Exception ex)
        {
            statusMessage = "Error deleting project. Please try again.";
            // Log the exception in a real application
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OpenProject(Guid projectId)
    {
        Navigation.NavigateTo($"/projects/{projectId}");
    }
}