using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HR.Api
{
    public static class TwoFactorApi
    {
        public static IEndpointRouteBuilder MapTwoFactorApi(this IEndpointRouteBuilder endpoints)
        {
            // Get 2FA status
            endpoints.MapGet("/api/2fa/status/{userId}", async (string userId, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null) return Results.NotFound();
                var hasAuthenticator = !string.IsNullOrEmpty(await userManager.GetAuthenticatorKeyAsync(user));
                var is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user);
                var recoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);
                return Results.Ok(new { hasAuthenticator, is2faEnabled, recoveryCodesLeft });
            });

            // Reset authenticator key
            endpoints.MapPost("/api/2fa/reset-authenticator/{userId}", async (string userId, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null) return Results.NotFound();
                await userManager.ResetAuthenticatorKeyAsync(user);
                await userManager.SetTwoFactorEnabledAsync(user, false);
                return Results.Ok();
            });

            // Forget browser
            endpoints.MapPost("/api/2fa/forget-browser/{userId}", async (string userId, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null) return Results.NotFound();
                await signInManager.ForgetTwoFactorClientAsync();
                return Results.Ok();
            });

            // Generate new recovery codes
            endpoints.MapPost("/api/2fa/generate-recovery-codes/{userId}", async (string userId, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null) return Results.NotFound();
                var codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                return Results.Ok(codes);
            });

            // Get 2FA enabled status for current user
            endpoints.MapGet("/api/twofactor/enabled", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var enabled = await userManager.GetTwoFactorEnabledAsync(user);
                return Results.Ok(enabled);
            });

            // Generate new recovery codes for current user
            endpoints.MapPost("/api/twofactor/generate-recovery-codes", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                return Results.Ok(codes);
            });

            return endpoints;
        }
    }
}
