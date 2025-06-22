using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace HR.Api
{
    public static class UserApi
    {
        public static IEndpointRouteBuilder MapUserApi(this IEndpointRouteBuilder endpoints)
        {
            // List users and their roles
            endpoints.MapGet("/api/users", async (UserManager<IdentityUser> userManager) =>
            {
                var users = userManager.Users.ToList();
                var userRoles = new Dictionary<string, List<string>>();
                foreach (var user in users)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    userRoles[user.Id] = roles.ToList();
                }
                return Results.Ok(new { users, userRoles });
            });

            // Delete user
            endpoints.MapDelete("/api/users/{id}", async (string id, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null) return Results.NotFound();
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            });

            // Reset user password
            endpoints.MapPost("/api/users/{id}/reset-password", async (string id, [FromBody] ResetPasswordRequest req, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null) return Results.NotFound();
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, req.NewPassword);
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            });

            // Get user claims
            endpoints.MapGet("/api/users/{id}/claims", async (string id, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null) return Results.NotFound();
                var claims = await userManager.GetClaimsAsync(user);
                return Results.Ok(claims);
            });

            // Add user claim
            endpoints.MapPost("/api/users/{id}/claims", async (string id, [FromBody] ClaimModel claim, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null) return Results.NotFound();
                var result = await userManager.AddClaimAsync(user, new Claim(claim.Type, claim.Value));
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            });

            // Remove user claim
            endpoints.MapDelete("/api/users/{id}/claims", async (string id, [FromBody] ClaimModel claim, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null) return Results.NotFound();
                var result = await userManager.RemoveClaimAsync(user, new Claim(claim.Type, claim.Value));
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            });

            // Confirm email
            endpoints.MapPost("/api/user/confirm-email", async ([FromBody] ConfirmEmailModel model, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(model.UserId);
                if (user == null) return Results.NotFound();
                var result = await userManager.ConfirmEmailAsync(user, model.Code);
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            });

            // Confirm email change
            endpoints.MapPost("/api/user/confirm-email-change", async ([FromBody] ConfirmEmailChangeModel model, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) =>
            {
                var user = await userManager.FindByIdAsync(model.UserId);
                if (user == null) return Results.NotFound();
                var result = await userManager.ChangeEmailAsync(user, model.Email, model.Code);
                if (!result.Succeeded) return Results.BadRequest(result.Errors);
                var setUserNameResult = await userManager.SetUserNameAsync(user, model.Email);
                if (!setUserNameResult.Succeeded) return Results.BadRequest(setUserNameResult.Errors);
                await signInManager.RefreshSignInAsync(user);
                return Results.Ok();
            });

            // Enable lockout
            endpoints.MapPost("/api/user/enable-lockout", async ([FromBody] EnableLockoutModel model, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    var valid = await userManager.CheckPasswordAsync(user, model.Password);
                    if (!valid) return Results.BadRequest("Incorrect password.");
                }
                var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                if (!result.Succeeded) return Results.BadRequest(result.Errors);
                await signInManager.SignOutAsync();
                return Results.Ok();
            });

            // Check if user has password (for lockout/delete)
            endpoints.MapGet("/api/user/has-password", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var hasPassword = await userManager.HasPasswordAsync(user);
                return Results.Ok(hasPassword);
            });

            return endpoints;
        }

        public class ResetPasswordRequest
        {
            [Required]
            public string NewPassword { get; set; } = string.Empty;
        }

        public class ClaimModel
        {
            [Required]
            public string Type { get; set; } = string.Empty;
            [Required]
            public string Value { get; set; } = string.Empty;
        }

        public class ConfirmEmailModel
        {
            public string UserId { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
        }
        public class ConfirmEmailChangeModel
        {
            public string UserId { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
        }
        public class EnableLockoutModel
        {
            public string? Password { get; set; }
        }
    }
}
