using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public interface IApiEndpoint
    {
        void MapApi(IEndpointRouteBuilder endpoints);
    }

    public class BenefitApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Implementation for mapping BenefitApi endpoints
        }
    }
}
