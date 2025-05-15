using VideoScripter.Features.Projects;

namespace Microsoft.Extensions.DependencyInjection;

public static class ProjectServiceExtensions
{
    public static IServiceCollection AddProjectServices(this IServiceCollection services)
    {
        services.AddScoped<ProjectHandler>();

        return services;
    }
}