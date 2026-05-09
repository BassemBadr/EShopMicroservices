using Carter;

namespace Ordering.API;

public static class DependancyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        var assembly = typeof(Program).Assembly;
        services.AddCarter(new DependencyContextAssemblyCatalog([assembly]));

        return services;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.MapCarter();

        return app;
    }
}
