using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace HR.Api
{
    public static class TwoFactorApi
    {
        public static IEndpointRouteBuilder MapTwoFactorApi(this IEndpointRouteBuilder endpoints)
        {
            // Get 2FA status for the current user
            endpoints.MapGet("/api/2fa/status", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var hasAuthenticator = !string.IsNullOrEmpty(await userManager.GetAuthenticatorKeyAsync(user));
                var is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user);
                var recoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);
                return Results.Ok(new { hasAuthenticator, is2faEnabled, recoveryCodesLeft });
            }).RequireAuthorization();

            // Get authenticator setup info (shared key and QR code URI)
            endpoints.MapGet("/api/2fa/setup", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();

                // Ensure the user has an authenticator key
                var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
                if (string.IsNullOrEmpty(unformattedKey))
                {
                    await userManager.ResetAuthenticatorKeyAsync(user);
                    unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
                }

                // Format the key for display
                string sharedKey = FormatKey(unformattedKey);

                // Generate QR code URI (otpauth://)
                string email = user.Email ?? user.UserName ?? "user";
                string issuer = "HRApp"; // Change to your app's name
                string encodedIssuer = Uri.EscapeDataString(issuer);
                string encodedEmail = Uri.EscapeDataString(email);
                string qrCodeUri = $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={unformattedKey}&issuer={encodedIssuer}&digits=6";

                return Results.Ok(new AuthenticatorSetupDto
                {
                    SharedKey = sharedKey,
                    QrCodeUri = qrCodeUri
                });
            }).RequireAuthorization();

            // Reset authenticator key for the current user
            endpoints.MapPost("/api/2fa/reset-authenticator", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                await userManager.ResetAuthenticatorKeyAsync(user);
                await userManager.SetTwoFactorEnabledAsync(user, false);
                return Results.Ok();
            }).RequireAuthorization();

            // Forget browser for the current user
            endpoints.MapPost("/api/2fa/forget-browser", async (SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                await signInManager.ForgetTwoFactorClientAsync();
                return Results.Ok();
            }).RequireAuthorization();

            // Generate new recovery codes for the current user
            endpoints.MapPost("/api/2fa/generate-recovery-codes", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var enabled = await userManager.GetTwoFactorEnabledAsync(user);
                if (!enabled) return Results.BadRequest("Two-factor authentication is not enabled for this user.");
                var codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                return Results.Ok(codes);
            }).RequireAuthorization();

            // Get 2FA enabled status for current user
            endpoints.MapGet("/api/2fa/enabled", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var enabled = await userManager.GetTwoFactorEnabledAsync(user);
                return Results.Ok(enabled);
            }).RequireAuthorization();

            endpoints.MapPost("/api/2fa/is2famachineremembered", async (
         [FromBody] IdentityUser user,
         UserManager<IdentityUser> userManager,
         SignInManager<IdentityUser> signInManager) =>
            {
                if (user == null)
                    return Results.NotFound();

                var isRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user);
                return Results.Ok(new MachineRememberedResponse { isRemembered = isRemembered });
            });

            // Enable 2FA for the current user
            endpoints.MapPost("/api/2fa/enable", async (
                [FromBody] Enable2faInputModel input,
                UserManager<IdentityUser> userManager,
                HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null)
                    return Results.Unauthorized();

                // Validate the verification code
                var isValid = await userManager.VerifyTwoFactorTokenAsync(
                    user,
                    userManager.Options.Tokens.AuthenticatorTokenProvider,
                    input.Code?.Replace(" ", string.Empty) ?? string.Empty);

                if (!isValid)
                    return Results.BadRequest("Invalid verification code.");

                var enableResult = await userManager.SetTwoFactorEnabledAsync(user, true);
                if (!enableResult.Succeeded)
                    return Results.BadRequest("Failed to enable two-factor authentication.");

                // Generate new recovery codes
                var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

                return Results.Ok(recoveryCodes.ToArray());
            }).RequireAuthorization();



        endpoints.MapPost("/api/2fa/disable", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null)
                    return Results.Unauthorized();

                var is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user);
                if (!is2faEnabled)
                    return Results.BadRequest("2FA is not currently enabled for your account.");

                var disable2faResult = await userManager.SetTwoFactorEnabledAsync(user, false);
                if (!disable2faResult.Succeeded)
                    return Results.BadRequest("Unexpected error occurred disabling 2FA.");
                return Results.Ok();
            }).RequireAuthorization();

            return endpoints;
        }



        // Helper to format the key for display
        private static string FormatKey(string unformattedKey)
        {
            var result = new System.Text.StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }
            return result.ToString().ToLowerInvariant();
        }
        public class Enable2faInputModel
        {
            public string Code { get; set; } = string.Empty;
        }
        public class AuthenticatorSetupDto
        {
            public string SharedKey { get; set; } = string.Empty;
            public string QrCodeUri { get; set; } = string.Empty;
        }

        public class MachineRememberedResponse
        {
            public bool isRemembered { get; set; }
        }
    }
}
