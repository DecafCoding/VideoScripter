using System.ComponentModel.DataAnnotations;

namespace VideoScripter.Features.Projects.Models;

public class ProjectModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Topic is required")]
    [StringLength(500, ErrorMessage = "Topic cannot exceed 500 characters")]
    public string Topic { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public int VideoCount { get; set; }
    public int ScriptCount { get; set; }
}

public class CreateProjectModel
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Topic is required")]
    [StringLength(500, ErrorMessage = "Topic cannot exceed 500 characters")]
    public string Topic { get; set; } = string.Empty;
}

public class EditProjectModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Topic is required")]
    [StringLength(500, ErrorMessage = "Topic cannot exceed 500 characters")]
    public string Topic { get; set; } = string.Empty;
}
