using Microsoft.AspNetCore.Routing;
using System.Reflection;
using HR.Api;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HR.Config
{
    public static class ApiEndpointRegistration
    {
        public static IServiceCollection AddEndpoints(
      this IServiceCollection services)
        {
            ServiceDescriptor[] serviceDescriptors = Assembly.GetExecutingAssembly()
                .DefinedTypes
                .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                               type.IsAssignableTo(typeof(IApiEndpoint)))
                .Select(type => ServiceDescriptor.Transient(typeof(IApiEndpoint), type))
                .ToArray();

            services.TryAddEnumerable(serviceDescriptors);

            return services;
        }

        public static IApplicationBuilder MapEndpoints(
   this WebApplication app,
   RouteGroupBuilder? routeGroupBuilder = null)
        {
            IEnumerable<IApiEndpoint> endpoints = app.Services
                .GetRequiredService<IEnumerable<IApiEndpoint>>();

            IEndpointRouteBuilder builder =
                routeGroupBuilder is null ? app : routeGroupBuilder;

            foreach (IApiEndpoint endpoint in endpoints)
            {
                endpoint.MapApi(builder);
            }

            return app;
        }


    }

}
