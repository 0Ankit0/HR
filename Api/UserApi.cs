using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static HR.Components.Account.Email;
using static HR.Components.Account.Phone;

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
            }).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

            // Delete user
            endpoints.MapDelete("/api/users/{id}", async (string id, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null) return Results.NotFound();
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            }).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

            // Add user claim
            endpoints.MapPost("/api/users/{id}/claims", async (
                string id,
                [FromBody] ClaimModel claim,
                UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null) return Results.NotFound();
                var result = await userManager.AddClaimAsync(user, new Claim(claim.Type, claim.Value));
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            }).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

            // Remove user claim
            endpoints.MapDelete("/api/users/{id}/claims", async (
                string id,
                [FromBody] ClaimModel claim,
                UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null) return Results.NotFound();
                var result = await userManager.RemoveClaimAsync(user, new Claim(claim.Type, claim.Value));
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            }).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

            // Reset user password
            endpoints.MapPost("/api/user/reset-password", async ([FromBody] ResetPasswordRequest req, UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, req.NewPassword);
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            }).RequireAuthorization();


            // Get user claims
            endpoints.MapGet("/api/user/claims", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var claims = await userManager.GetClaimsAsync(user);
                return Results.Ok(claims);
            }).RequireAuthorization();

            // Add user claim
            endpoints.MapPost("/api/user/claims", async ([FromBody] ClaimModel claim, UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var result = await userManager.AddClaimAsync(user, new Claim(claim.Type, claim.Value));
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            }).RequireAuthorization();

            // Remove user claim
            endpoints.MapDelete("/api/user/claims", async ([FromBody] ClaimModel claim, UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var result = await userManager.RemoveClaimAsync(user, new Claim(claim.Type, claim.Value));
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            }).RequireAuthorization();

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
                //var setUserNameResult = await userManager.SetUserNameAsync(user, model.Email);
                //if (!setUserNameResult.Succeeded) return Results.BadRequest(setUserNameResult.Errors);
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

            // Change email: send confirmation link to new email
            endpoints.MapPost("/api/user/changeemail", async (
                [FromBody] ChangeEmailModel model,
                UserManager<IdentityUser> userManager,
                IEmailSender emailSender,
                HttpContext context) =>
            {
                var user = await userManager.FindByEmailAsync(model.CurrentEmail);
                if (user == null)
                    return Results.NotFound("User not found.");

                if (model.NewEmail == user.Email)
                    return Results.Ok("Your email is unchanged.");

                var userId = await userManager.GetUserIdAsync(user);
                var code = await userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
                code = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(code));

                var request = context.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var callbackUrl = $"{baseUrl}/account/confirmemailchange?userId={Uri.EscapeDataString(userId)}&email={Uri.EscapeDataString(model.NewEmail)}&code={Uri.EscapeDataString(code)}";

                await emailSender.SendEmailAsync(
                    model.NewEmail,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

                return Results.Ok("Confirmation link to change email sent. Please check your email.");
            });

            // Change phone
            endpoints.MapPost("/api/user/changephone", async ([FromBody] ChangePhoneModel model, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null) return Results.NotFound();
                var result = await userManager.SetPhoneNumberAsync(user, model.NewPhone);
                if (!result.Succeeded) return Results.BadRequest(result.Errors);
                return Results.Ok();
            });

            // Get current user's email
            endpoints.MapGet("/api/user/email", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                return Results.Ok(new EmailDto {Email= user.Email,IsConfirmed=user.EmailConfirmed });
            });

            // Get current user's phone number
            endpoints.MapGet("/api/user/phone", async (UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                return Results.Ok(new PhoneDto {PhoneNumber= user.PhoneNumber,IsConfirmed=user.PhoneNumberConfirmed });
            });

            // Send email verification
            endpoints.MapPost("/api/user/sendemailverification", async (
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();

                // Use the user's current email for verification
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                code = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(code));
                var userId = user.Id;
                var request = context.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var callbackUrl = $"{baseUrl}/account/confirmemail?userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}";

                await emailSender.SendEmailAsync(
                    user.Email,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

                return Results.Ok();
            });

            // Send phone verification SMS
            endpoints.MapPost("/api/user/sendphonesms", async (UserManager<IdentityUser> userManager, ISmsSender smsSender, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();

                // Generate a phone number verification token
                var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                var message = $"Your verification code is: {code}";

                await smsSender.SendSmsAsync(user.PhoneNumber, message);

                return Results.Ok();
            });

            // Confirm phone number
            endpoints.MapPost("/api/user/confirmphone", async ([FromBody] ConfirmPhoneModel model, UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var result = await userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);
                if (!result.Succeeded) return Results.BadRequest(result.Errors);
                return Results.Ok();
            });

            // Change username
            endpoints.MapPost("/api/user/changeusername", async ([FromBody] ChangeUsernameModel model, UserManager<IdentityUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                var result = await userManager.SetUserNameAsync(user, model.NewUsername);
                if (!result.Succeeded) return Results.BadRequest(result.Errors);
                return Results.Ok();
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
        public class ConfirmPhoneModel { public string PhoneNumber { get; set; } = string.Empty; public string Code { get; set; } = string.Empty; }
        public class ChangeUsernameModel { public string NewUsername { get; set; } = string.Empty; }
    }
}
