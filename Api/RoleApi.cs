using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HR.Api
{
    public static class RoleApi
    {
        public static IEndpointRouteBuilder MapRoleApi(this IEndpointRouteBuilder endpoints)
        {
            // List roles
            endpoints.MapGet("/api/roles", async (RoleManager<IdentityRole> roleManager) =>
            {
                var roles = roleManager.Roles.ToList();
                return Results.Ok(roles);
            });

            // Create role
            endpoints.MapPost("/api/roles", async ([FromBody] RoleModel model, RoleManager<IdentityRole> roleManager) =>
            {
                var result = await roleManager.CreateAsync(new IdentityRole(model.Name));
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            });

            // Delete role
            endpoints.MapDelete("/api/roles/{name}", async (string name, RoleManager<IdentityRole> roleManager) =>
            {
                var role = await roleManager.FindByNameAsync(name);
                if (role == null) return Results.NotFound();
                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            });

            // Assign role to user
            endpoints.MapPost("/api/roles/assign", async ([FromBody] RoleAssignModel model, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(model.UserId);
                if (user == null) return Results.NotFound();
                var result = await userManager.AddToRoleAsync(user, model.RoleName);
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            });

            // Remove role from user
            endpoints.MapPost("/api/roles/remove", async ([FromBody] RoleAssignModel model, UserManager<IdentityUser> userManager) =>
            {
                var user = await userManager.FindByIdAsync(model.UserId);
                if (user == null) return Results.NotFound();
                var result = await userManager.RemoveFromRoleAsync(user, model.RoleName);
                if (result.Succeeded) return Results.Ok();
                return Results.BadRequest(result.Errors);
            });

            // Get users in a role
            endpoints.MapGet("/api/roles/{roleId}/users", async (string roleId, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager) =>
            {
                var role = await roleManager.FindByIdAsync(roleId);
                if (role == null) return Results.NotFound();
                var users = userManager.Users.ToList();
                var usersInRole = new List<object>();
                foreach (var user in users)
                {
                    if (await userManager.IsInRoleAsync(user, role.Name))
                        usersInRole.Add(new { user.Id, user.UserName, user.Email });
                }
                return Results.Ok(usersInRole);
            });

            // Get role by id
            endpoints.MapGet("/api/roles/{roleId}", async (string roleId, RoleManager<IdentityRole> roleManager) =>
            {
                var role = await roleManager.FindByIdAsync(roleId);
                if (role == null) return Results.NotFound();
                return Results.Ok(role);
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
            [Required]
            public string UserId { get; set; } = string.Empty;
            [Required]
            public string RoleName { get; set; } = string.Empty;
        }
    }
}
