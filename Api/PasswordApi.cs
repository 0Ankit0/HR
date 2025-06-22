using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HR.Api
{
    public static class PasswordApi
    {
        public static IEndpointRouteBuilder MapPasswordApi(this IEndpointRouteBuilder endpoints)
        {
            // Set password for user (if not set)
            endpoints.MapPost("/api/password/set/{userId}", async (string userId, [FromBody] SetPasswordModel model, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) =>
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null) return Results.NotFound();
                var hasPassword = await userManager.HasPasswordAsync(user);
                if (hasPassword) return Results.BadRequest("User already has a password.");
                var result = await userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await signInManager.RefreshSignInAsync(user);
                    return Results.Ok();
                }
                return Results.BadRequest(result.Errors);
            });

            // Check if user has password
            endpoints.MapGet("/api/password/has/{userId}", async (string userId, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null) return Results.NotFound();
                var hasPassword = await userManager.HasPasswordAsync(user);
                return Results.Ok(hasPassword);
            });

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
            });

            // Forgot password (send reset email)
            endpoints.MapPost("/api/password/forgot", async ([FromBody] ForgotPasswordModel model, UserManager<IdentityUser> userManager, IEmailSender<IdentityUser> emailSender) =>
            {
                var user = await userManager.FindByEmailAsync(model.Email!);
                if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
                    return Results.Ok(); // Don't reveal user existence
                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                // TODO: Generate callback URL and send email
                // await emailSender.SendEmailAsync(...)
                return Results.Ok();
            });

            // Reset password
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
