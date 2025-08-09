using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HR.Api
{
    public static class RoleApi
    {
        public static IEndpointRouteBuilder MapRoleApi(this IEndpointRouteBuilder endpoints)
        {
            // List roles as RoleDto
            endpoints.MapGet("/api/roles", async (RoleManager<IdentityRole> roleManager) =>
            {
                var roles = roleManager.Roles.Select(r => new RoleDto { Id = r.Id, Name = r.Name }).ToList();
                return Results.Ok(roles);
            });

            // Create role and return RoleDto
            endpoints.MapPost("/api/roles", async ([FromBody] RoleModel model, RoleManager<IdentityRole> roleManager) =>
            {
                var identityRole = new IdentityRole(model.Name);
                var result = await roleManager.CreateAsync(identityRole);
                if (result.Succeeded)
                    return Results.Ok(new RoleDto { Id = identityRole.Id, Name = identityRole.Name });
                return Results.BadRequest(result.Errors);
            });

            // Delete role
            endpoints.MapDelete("/api/roles/{roleId}", async (string roleId, RoleManager<IdentityRole> roleManager) =>
            {
                var role = await roleManager.FindByIdAsync(roleId);
                if (role == null) return Results.NotFound();
                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            });

            // Assign role to user
            endpoints.MapPost("/api/roles/assign", async (
                [FromBody] RoleAssignModel model,
                UserManager<IdentityUser> userManager,
                RoleManager<IdentityRole> roleManager) =>
            {
                if (string.IsNullOrWhiteSpace(model.UserEmail) || string.IsNullOrWhiteSpace(model.RoleId))
                    return Results.BadRequest("User email and role ID are required.");

                var user = await userManager.FindByEmailAsync(model.UserEmail);
                if (user == null)
                    return Results.NotFound("User not found.");

                var role = await roleManager.FindByIdAsync(model.RoleId);
                if (role == null)
                    return Results.NotFound("Role not found.");

                var result = await userManager.AddToRoleAsync(user, role.Name);
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            });

            // Remove role from user
            endpoints.MapPost("/api/roles/remove", async (
                [FromBody] RoleAssignModel model,
                UserManager<IdentityUser> userManager,
                RoleManager<IdentityRole> roleManager) =>
            {
                if (string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.RoleId))
                    return Results.BadRequest("User ID and role ID are required.");

                var user = await userManager.FindByIdAsync(model.UserId);
                if (user == null)
                    return Results.NotFound("User not found.");

                var role = await roleManager.FindByIdAsync(model.RoleId);
                if (role == null)
                    return Results.NotFound("Role not found.");

                var result = await userManager.RemoveFromRoleAsync(user, role.Name);
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            });

            // Get users in a role and return List<UserDto>
            endpoints.MapGet("/api/roles/{roleId}/users", async (string roleId, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager) =>
            {
                var role = await roleManager.FindByIdAsync(roleId);
                if (role == null) return Results.NotFound();
                var users = userManager.Users.ToList();
                var usersInRole = new List<UserDto>();
                foreach (var user in users)
                {
                    if (await userManager.IsInRoleAsync(user, role.Name))
                        usersInRole.Add(new UserDto { Id = user.Id, Email = user.Email });
                }
                return Results.Ok(usersInRole);
            });

            // Get role by id and return RoleDto
            endpoints.MapGet("/api/roles/{roleId}", async (string roleId, RoleManager<IdentityRole> roleManager) =>
            {
                var role = await roleManager.FindByIdAsync(roleId);
                if (role == null) return Results.NotFound();
                return Results.Ok(new RoleDto { Id = role.Id, Name = role.Name });
            });

            // Update role name
            endpoints.MapPut("/api/roles/{roleId}", async (string roleId, [FromBody] RoleDto model, RoleManager<IdentityRole> roleManager) =>
            {
                var role = await roleManager.FindByIdAsync(roleId);
                if (role == null) return Results.NotFound();
                role.Name = model.Name;
                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded) return Results.Ok(new RoleDto { Id = role.Id, Name = role.Name });
                return Results.BadRequest(result.Errors);
            });

            // Get all claims for a role
            endpoints.MapGet("/api/roles/{roleId}/claims", async (string roleId, RoleManager<IdentityRole> roleManager) =>
            {
                var role = await roleManager.FindByIdAsync(roleId);
                if (role == null) return Results.NotFound();
                var claims = await roleManager.GetClaimsAsync(role);
                var claimDtos = claims.Select(c => new ClaimDto { Type = c.Type, Value = c.Value }).ToList();
                return Results.Ok(claimDtos);
            });

            // Replace all claims for a role
            endpoints.MapPut("/api/roles/{roleId}/claims", async (string roleId, [FromBody] List<ClaimDto> claimDtos, RoleManager<IdentityRole> roleManager) =>
            {
                var role = await roleManager.FindByIdAsync(roleId);
                if (role == null) return Results.NotFound();

                // Remove all existing claims
                var existingClaims = await roleManager.GetClaimsAsync(role);
                foreach (var claim in existingClaims)
                {
                    await roleManager.RemoveClaimAsync(role, claim);
                }

                // Add new claims
                foreach (var dto in claimDtos)
                {
                    if (!string.IsNullOrWhiteSpace(dto.Type) && !string.IsNullOrWhiteSpace(dto.Value))
                        await roleManager.AddClaimAsync(role, new Claim(dto.Type, dto.Value));
                }

                return Results.Ok();
            });

            return endpoints;
        }

        public class RoleModel
        {
            [Required]
            public string Name { get; set; } = string.Empty;
        }
        public class RoleAssignModel
        {
            public string? UserId { get; set; }
            public string? UserEmail { get; set; }
            public string? RoleId { get; set; }
            [Required]
            public string RoleName { get; set; } = string.Empty;
        }
        public class RoleResponse
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
        }

        public class RoleDto
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }
        public class UserDto
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class ClaimDto
        {
            public string Type { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }
    }
}
