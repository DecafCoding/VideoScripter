using Microsoft.EntityFrameworkCore;
using VideoScripter.Data;
using VideoScripter.Data.Entities;
using VideoScripter.Features.Projects.Models;

namespace VideoScripter.Features.Projects;

public class ProjectHandler
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProjectHandler> _logger;

    public ProjectHandler(ApplicationDbContext context, ILogger<ProjectHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ProjectModel>> GetUserProjectsAsync(string userId)
    {
        try
        {
            var projects = await _context.Set<Project>()
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .Select(p => new ProjectModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Topic = p.Topic,
                    CreatedAt = p.CreatedAt,
                    LastModifiedAt = p.LastModifiedAt,
                    VideoCount = p.Videos.Count(v => !v.IsDeleted),
                    ScriptCount = p.Scripts.Count(s => !s.IsDeleted)
                })
                .OrderByDescending(p => p.LastModifiedAt)
                .ToListAsync();

            return projects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects for user {UserId}", userId);
            throw;
        }
    }

    public async Task<ProjectModel?> GetProjectByIdAsync(Guid projectId, string userId)
    {
        try
        {
            var project = await _context.Set<Project>()
                .Where(p => p.Id == projectId && p.UserId == userId && !p.IsDeleted)
                .Select(p => new ProjectModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Topic = p.Topic,
                    CreatedAt = p.CreatedAt,
                    LastModifiedAt = p.LastModifiedAt,
                    VideoCount = p.Videos.Count(v => !v.IsDeleted),
                    ScriptCount = p.Scripts.Count(s => !s.IsDeleted)
                })
                .FirstOrDefaultAsync();

            return project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project {ProjectId} for user {UserId}", projectId, userId);
            throw;
        }
    }

    public async Task<ProjectModel> CreateProjectAsync(CreateProjectModel model, string userId)
    {
        try
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Topic = model.Topic,
                UserId = userId,
                CreatedBy = userId,
                LastModifiedBy = userId
            };

            _context.Set<Project>().Add(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created project {ProjectId} for user {UserId}", project.Id, userId);

            return new ProjectModel
            {
                Id = project.Id,
                Name = project.Name,
                Topic = project.Topic,
                CreatedAt = project.CreatedAt,
                LastModifiedAt = project.LastModifiedAt,
                VideoCount = 0,
                ScriptCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project for user {UserId}", userId);
            throw;
        }
    }

    public async Task<ProjectModel?> UpdateProjectAsync(EditProjectModel model, string userId)
    {
        try
        {
            var project = await _context.Set<Project>()
                .FirstOrDefaultAsync(p => p.Id == model.Id && p.UserId == userId && !p.IsDeleted);

            if (project == null)
            {
                return null;
            }

            project.Name = model.Name;
            project.Topic = model.Topic;
            project.LastModifiedAt = DateTime.UtcNow;
            project.LastModifiedBy = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated project {ProjectId} for user {UserId}", project.Id, userId);

            return new ProjectModel
            {
                Id = project.Id,
                Name = project.Name,
                Topic = project.Topic,
                CreatedAt = project.CreatedAt,
                LastModifiedAt = project.LastModifiedAt,
                VideoCount = project.Videos.Count(v => !v.IsDeleted),
                ScriptCount = project.Scripts.Count(s => !s.IsDeleted)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId} for user {UserId}", model.Id, userId);
            throw;
        }
    }

    public async Task<bool> DeleteProjectAsync(Guid projectId, string userId)
    {
        try
        {
            var project = await _context.Set<Project>()
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId && !p.IsDeleted);

            if (project == null)
            {
                return false;
            }

            // Soft delete - mark as deleted instead of removing from database
            project.IsDeleted = true;
            project.LastModifiedAt = DateTime.UtcNow;
            project.LastModifiedBy = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted project {ProjectId} for user {UserId}", projectId, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId} for user {UserId}", projectId, userId);
            throw;
        }
    }

    public async Task<bool> ProjectExistsAsync(Guid projectId, string userId)
    {
        try
        {
            return await _context.Set<Project>()
                .AnyAsync(p => p.Id == projectId && p.UserId == userId && !p.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if project {ProjectId} exists for user {UserId}", projectId, userId);
            throw;
        }
    }
}