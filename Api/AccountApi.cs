using HR.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using static HR.Components.Account.ExternalLogins;

namespace HR.Api
{
    public static class AccountApi
    {
        public static IEndpointRouteBuilder MapAccountApi(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/api/account/login", async ([FromForm] LoginRequest loginRequest, SignInManager<IdentityUser> signInManager, HttpContext context) =>
            {
                var result = await signInManager.PasswordSignInAsync(loginRequest.Email, loginRequest.Password, false, false);
                if (result.Succeeded)
                {
                    return Results.Redirect("/");
                }
                else if (result.RequiresTwoFactor)
                {
                    return Results.Redirect($"/account/loginwith2fa");
                }
                else if (result.IsLockedOut)
                {
                    return Results.Redirect($"/account/lockout");
                }
                else if (result.IsNotAllowed)
                {
                    return Results.Redirect($"/account/login?error={Uri.EscapeDataString("Your account is not allowed to sign in. Please contact support.")}");
                }
                else
                {
                    return Results.Redirect($"/account/login?error={Uri.EscapeDataString("Invalid login attempt.")}");
                }
            }).DisableAntiforgery();

            endpoints.MapPost("/api/account/loginwith2fa", async (
        [FromForm] TwoFactorLoginRequest model,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        HttpContext httpContext) =>
            {
                var result = await signInManager.TwoFactorAuthenticatorSignInAsync(model.TwoFactorCode, model.RememberMe, model.RememberMachine);
                if (result.Succeeded)
                {
                    return Results.Redirect(model?.ReturnUrl ?? "/");
                }
                return Results.Redirect("/account/login?error=Invalid");

            }).DisableAntiforgery();
            endpoints.MapGet("/account/externalLoginLink", async (
                HttpContext httpContext,
                SignInManager<IdentityUser> signInManager,
                UserManager<IdentityUser> userManager,
                [FromQuery] string provider,
                [FromQuery] string returnUrl) =>
            {
                // Clear the existing external cookie to ensure a clean login process
                await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                // Set up the callback URL for after the external provider returns
                var callbackUrl = $"/account/externalLoginLink/callback?returnUrl={Uri.EscapeDataString(returnUrl)}";
                var userId = userManager.GetUserId(httpContext.User);
                var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl, userId);

                // Initiate the external login challenge
                return Results.Challenge(properties, new[] { provider });
            });

            endpoints.MapGet("/account/externalLoginLink/callback", async (
                HttpContext httpContext,
                SignInManager<IdentityUser> signInManager,
                UserManager<IdentityUser> userManager,
                [FromQuery] string returnUrl) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Redirect($"{returnUrl}?statusMessage=User%20not%20found.");
                }

                var userId = await userManager.GetUserIdAsync(user);
                var info = await signInManager.GetExternalLoginInfoAsync(userId);
                if (info == null)
                {
                    return Results.Redirect($"{returnUrl}?statusMessage=Unexpected%20error%20loading%20external%20login%20info.");
                }

                var result = await userManager.AddLoginAsync(user, info);
                if (!result.Succeeded)
                {
                    return Results.Redirect($"{returnUrl}?statusMessage=The%20external%20login%20was%20not%20added.%20External%20logins%20can%20only%20be%20associated%20with%20one%20account.");
                }

                // Clear the existing external cookie to ensure a clean login process
                await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                return Results.Redirect($"{returnUrl}?statusMessage=The%20external%20login%20was%20added.");
            });

            endpoints.MapGet("/api/account/refreshsignin", async (
                HttpContext context,
                UserManager<IdentityUser> userManager,
                SignInManager<IdentityUser> signInManager) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                await signInManager.RefreshSignInAsync(user);
                return Results.Ok(new { message = "Sign-in refreshed." });
            }).RequireAuthorization();

            endpoints.MapGet("/api/account/logout", async (
            SignInManager<IdentityUser> signInManager,
            HttpContext context) =>
                {
                    await signInManager.SignOutAsync();
                    return Results.Redirect($"/account/login?error={Uri.EscapeDataString("You have been logged out. Please login to proceed.")}");
                });

            //endpoints.MapPost("/api/account/logout", async (
            //SignInManager<IdentityUser> signInManager,
            //HttpContext context) =>
            //    {
            //        await signInManager.SignOutAsync();
            //        return Results.Ok(new { message = "You have been logged out." });
            //    }).RequireAuthorization();

           
            endpoints.MapPost("/api/account/loginwithrecoverycode", async (
        [FromForm] RecoveryCodeLoginRequest model,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        HttpContext httpContext) =>
            {
                var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(model.RecoveryCode);
                if (result.Succeeded)
                {
                    return Results.Redirect(model?.ReturnUrl ?? "/");
                }
                else if (result.IsLockedOut)
                {
                    return Results.Redirect("/account/lockout");
                }
                else
                {
                    // Redirect back with error message
                    return Results.Redirect($"/account/loginwithrecoverycode?message={Uri.EscapeDataString("Invalid recovery code entered.")}");
                }
            }).DisableAntiforgery();

            // Register user
            endpoints.MapPost("/api/account/register", async ([FromBody] RegisterModel model, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) =>
            {
                var user = new IdentityUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNo
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded) return Results.BadRequest(result.Errors);
                await signInManager.SignInAsync(user, isPersistent: false);
                return Results.Ok();
            });

            // Get profile
            endpoints.MapGet("/api/account/profile", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                return Results.Ok(new { user.UserName, user.Email, user.PhoneNumber });
            });

            // Get personal data
            endpoints.MapGet("/api/account/personaldata", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                return Results.Ok(new { user.UserName, user.Email, user.PhoneNumber });
            });

            // Download personal data
            endpoints.MapGet("/api/account/downloadpersonaldata", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                return Results.Ok(System.Text.Json.JsonSerializer.Serialize(new { user.UserName, user.Email, user.PhoneNumber }));
            });

            // Delete personal data
            endpoints.MapPost("/api/account/deletepersonaldata", async ([FromBody] DeletePersonalDataModel model, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    var valid = await userManager.CheckPasswordAsync(user, model.Password);
                    if (!valid) return Results.BadRequest("Incorrect password.");
                }
                var result = await userManager.DeleteAsync(user);
                if (!result.Succeeded) return Results.BadRequest(result.Errors);
                await signInManager.SignOutAsync();
                return Results.Ok();
            });

            // Resend email confirmation
            endpoints.MapPost("/api/account/resendemailconfirmation", async ([FromBody] ResendEmailConfirmationModel model, UserManager<IdentityUser> userManager, IEmailSender emailSender, HttpContext context) =>
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null) return Results.Ok();

                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                code = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(code));
                var userId = user.Id;
                var request = context.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var confirmationUrl = $"{baseUrl}/account/confirmemail?userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}";

                await emailSender.SendEmailAsync(
                    user.Email,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{confirmationUrl}'>clicking here</a>.");

                return Results.Ok();
            });


            // Unlock account
            endpoints.MapPost("/api/account/unlock", async (
                [FromBody] UnlockModel model,
                UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null) return Results.NotFound();

                // No decoding needed, use the code directly
                var isValid = await userManager.VerifyUserTokenAsync(
                    user, TokenOptions.DefaultProvider, "RemoveLockout", model.Code);

                if (!isValid)
                    return Results.BadRequest("Invalid or expired unlock code.");

                await userManager.SetLockoutEndDateAsync(user, null);
                return Results.Ok();
            });

            // Request unlock
            endpoints.MapPost("/api/account/requestunlock", async (
    [FromBody] RequestUnlockModel model,
    UserManager<IdentityUser> userManager,
    IEmailSender emailSender,
    HttpContext context) =>
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null) return Results.Ok();

                var code = await userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "RemoveLockout");
                var encodedCode = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(code));
                var request = context.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var unlockUrl = $"{baseUrl}/account/unlock?email={Uri.EscapeDataString(model.Email)}&code={Uri.EscapeDataString(encodedCode)}";

                var emailBody = $@"
        <p>Your account is locked out. Click the link below to unlock your account:</p>
        <p><a href=""{unlockUrl}"">Unlock Account</a></p>
        <p>If you did not request this, you can ignore this email.</p>
    ";

                await emailSender.SendEmailAsync(
                    user.Email,
                    "Unlock your account",
                    emailBody);

                return Results.Ok();
            });

            // List available external login providers
            endpoints.MapGet("/api/account/externallogins", async (SignInManager<IdentityUser> signInManager) =>
            {
                var providers = (await signInManager.GetExternalAuthenticationSchemesAsync()).Select(s => new { s.Name, s.DisplayName });
                return Results.Ok(providers);
            });

            // List/manage linked external logins for current user
            endpoints.MapGet("/api/account/externallogins/manage", async (SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();

                var logins = await userManager.GetLoginsAsync(user);
                var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();

                var currentLogins = logins.Select(l => new ExternalLoginDto
                {
                    Name = l.LoginProvider,
                    DisplayName = l.ProviderDisplayName ?? l.LoginProvider,
                    ProviderDisplayName = l.ProviderDisplayName,
                    LoginProvider = l.LoginProvider,
                    ProviderKey = l.ProviderKey
                }).ToList();

                var otherLogins = schemes
                    .Where(s => !logins.Any(l => l.LoginProvider == s.Name))
                    .Select(s => new ExternalLoginDto
                    {
                        Name = s.Name,
                        DisplayName = s.DisplayName ?? s.Name,
                        ProviderDisplayName = s.DisplayName,
                        LoginProvider = (string?)null,
                        ProviderKey = (string?)null
                    }).ToList();

                return Results.Ok(new ExternalLoginsResult
                {
                    CurrentLogins = currentLogins,
                    OtherLogins = otherLogins
                });
            });

            // Remove an external login
            endpoints.MapPost("/api/account/removelogin", async ([FromBody] RemoveLoginModel model, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var result = await userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
                if (!result.Succeeded) return Results.BadRequest(result.Errors);
                await signInManager.RefreshSignInAsync(user);
                return Results.Ok();
            });

            // Link a new external login (placeholder, actual flow is handled by OIDC redirect)
            endpoints.MapPost("/api/account/linklogin", async ([FromBody] LinkLoginModel model, UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                // This would normally start the OIDC challenge
                return Results.Ok();
            });


            // Delete personal data (DELETE)
            endpoints.MapDelete("/api/account/personaldata", async (UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var result = await userManager.DeleteAsync(user);
                if (!result.Succeeded) return Results.BadRequest(result.Errors);
                await signInManager.SignOutAsync();
                return Results.Ok();
            });
            endpoints.MapGet("/api/account/registerconfirmation", async (
    [FromQuery] string email,
    UserManager<IdentityUser> userManager,
    HttpContext context) =>
            {
#if DEBUG
                if (string.IsNullOrEmpty(email))
                    return Results.BadRequest();

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                    return Results.NotFound();

                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                code = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(code));
                var userId = user.Id;
                var request = context.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var confirmationUrl = $"{baseUrl}/account/confirmemail?userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}";

                return Results.Ok(new { ConfirmationUrl = confirmationUrl });
#else
    return Results.NotFound();
#endif
            });

            // Reset password endpoint
            endpoints.MapPost("/api/account/resetpassword", async (
                [FromBody] ResetPasswordModel model,
                UserManager<IdentityUser> userManager) =>
            {
                if (string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.Password) ||
                    string.IsNullOrWhiteSpace(model.ConfirmPassword) ||
                    string.IsNullOrWhiteSpace(model.Code))
                {
                    return Results.BadRequest("All fields are required.");
                }

                if (model.Password != model.ConfirmPassword)
                {
                    return Results.BadRequest("Passwords do not match.");
                }

                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Do not reveal that the user does not exist
                    return Results.Ok();
                }

                var result = await userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return Results.Ok();
                }
                return Results.BadRequest(string.Join(" ", result.Errors.Select(e => e.Description)));
            });

            return endpoints;
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class RecoveryCodeLoginRequest
    {
        public string RecoveryCode { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
    }
    public class RequestLockoutCodeRequest
    {
        public string Email { get; set; } = string.Empty;
    }
    public class MachineRememberedResponse
    {
        public bool isRemembered { get; set; }
    }
    public class RemoveLockoutRequest { public string? Code { get; set; } }
    public class RegisterModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class ChangeEmailModel
    {
        public string CurrentEmail { get; set; } = string.Empty;
        public string NewEmail { get; set; } = string.Empty;
    }
    public class ChangePhoneModel
    {
        public string Email { get; set; } = string.Empty;
        public string NewPhone { get; set; } = string.Empty;
    }
    public class DeletePersonalDataModel
    {
        public string? Password { get; set; }
    }
    public class ResendEmailConfirmationModel
    {
        public string Email { get; set; } = string.Empty;
    }
    public class UnlockModel
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; 
    }
    public class RequestUnlockModel
    {
        public string Email { get; set; } = string.Empty;
    }
    public class RemoveLoginModel { public string LoginProvider { get; set; } = string.Empty; public string ProviderKey { get; set; } = string.Empty; }
    public class LinkLoginModel { public string Provider { get; set; } = string.Empty; }
    public class ConfirmPhoneModel { public string PhoneNumber { get; set; } = string.Empty; public string Code { get; set; } = string.Empty; }
    public class ChangeUsernameModel { public string NewUsername { get; set; } = string.Empty; }
    public class ResetPasswordModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}