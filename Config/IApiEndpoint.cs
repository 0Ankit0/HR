using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public interface IApiEndpoint
    {
        void MapApi(IEndpointRouteBuilder endpoints);
    }
}
