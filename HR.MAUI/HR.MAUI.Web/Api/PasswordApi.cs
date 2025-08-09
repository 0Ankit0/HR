using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HR.Api
{
    public static class PasswordApi
    {
        public static IEndpointRouteBuilder MapPasswordApi(this IEndpointRouteBuilder endpoints)
        {
            // Set password for current user (if not set)
            endpoints.MapPost("/api/password/set", async ([FromBody] SetPasswordModel model, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var hasPassword = await userManager.HasPasswordAsync(user);
                if (hasPassword) return Results.BadRequest("User already has a password.");
                var result = await userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await signInManager.RefreshSignInAsync(user);
                    return Results.Ok();
                }
                return Results.BadRequest(result.Errors);
            }).RequireAuthorization();

            // Check if current user has password
            endpoints.MapGet("/api/password/has-password", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var hasPassword = await userManager.HasPasswordAsync(user);
                return Results.Ok(hasPassword);
            }).RequireAuthorization();

            // Change password for user 
            endpoints.MapPost("/api/password/change", async ([FromBody] ChangePasswordModel model, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var result = await userManager.ChangePasswordAsync(user, model.OldPassword!, model.NewPassword!);
                if (!result.Succeeded)
                    return Results.BadRequest(result.Errors.Select(e => e.Description));
                await signInManager.RefreshSignInAsync(user);
                return Results.Ok();
            }).RequireAuthorization();

            // Forgot password
            endpoints.MapPost("/api/password/forgot", async (
     [FromBody] ForgotPasswordModel model,
     UserManager<IdentityUser> userManager,
     IEmailSender emailSender,
     IHttpContextAccessor httpContextAccessor) =>
            {
                var user = await userManager.FindByEmailAsync(model.Email!);
                if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
                    return Results.Ok(); // Don't reveal user existence

                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                code = System.Web.HttpUtility.UrlEncode(
                    System.Text.Encoding.UTF8.GetString(
                        System.Text.Encoding.UTF8.GetBytes(code)
                    )
                );
                var request = httpContextAccessor.HttpContext!.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var callbackUrl = $"{baseUrl}/account/resetpassword?code={code}";

                await emailSender.SendEmailAsync(
                    user.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");

                return Results.Ok();
            });

            // Reset password - unchanged
            endpoints.MapPost("/api/account/resetpassword", async ([FromBody] ResetPasswordModel model, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) =>
            {
                var user = await userManager.FindByEmailAsync(model.Email!);
                if (user == null) return Results.BadRequest("User not found.");
                var result = await userManager.ResetPasswordAsync(user, model.Code!, model.Password!);
                if (!result.Succeeded)
                    return Results.BadRequest(result.Errors.Select(e => e.Description));
                await signInManager.RefreshSignInAsync(user);
                return Results.Ok();
            });

            return endpoints;
        }
        public class SetPasswordModel
        {
            [Required]
            [MinLength(6)]
            public string? NewPassword { get; set; }
            [Required]
            [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
            public string? ConfirmPassword { get; set; }
        }
        public class ChangePasswordModel
        {
            [Required]
            public string? OldPassword { get; set; }
            [Required]
            [MinLength(6)]
            public string? NewPassword { get; set; }
            [Required]
            [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
            public string? ConfirmPassword { get; set; }
        }
        public class ForgotPasswordModel
        {
            [Required]
            [EmailAddress]
            public string? Email { get; set; }
        }
        public class ResetPasswordModel
        {
            [Required]
            [EmailAddress]
            public string? Email { get; set; }
            [Required]
            public string? Password { get; set; }
            [Required]
            public string? Code { get; set; }
        }
    }
}
