using Microsoft.AspNetCore.Components.Authorization;

namespace HR.Config;

public class AuthenticationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Get the auth cookie from the current HTTP context
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            // Copy the authentication cookie to the outgoing request
            var cookies = context.Request.Headers.Cookie;
            if (!string.IsNullOrEmpty(cookies))
            {
                // Explicitly cast cookies to string to resolve ambiguity
                request.Headers.Add("Cookie", cookies.ToString());
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}