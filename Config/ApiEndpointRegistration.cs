using Microsoft.AspNetCore.Routing;
using System.Reflection;
using HR.Api;

namespace HR.Config
{
    public static class ApiEndpointRegistration
    {
        public static void RegisterAllApiEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var apiEndpointType = typeof(IApiEndpoint);
            var apiTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => apiEndpointType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in apiTypes)
            {
                if (Activator.CreateInstance(type) is IApiEndpoint api)
                {
                    api.MapApi(endpoints);
                }
            }
        }
    }
}
