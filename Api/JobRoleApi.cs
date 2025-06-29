using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class JobRoleApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/jobroles", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.JobRoles.Where(r => !r.IsDeleted);
                // Search by name
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(r => r.Role_Name.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(r => new HR.Models.JobRoleResponse
                    {
                        JobRole_ID = r.JobRole_ID,
                        Role_Name = r.Role_Name,
                        Role_Description = r.Role_Description
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            endpoints.MapGet("/api/jobroles/{id}", async (int id, AuthDbContext db) =>
                await db.JobRoles.FindAsync(id) is JobRole r ? Results.Ok(new HR.Models.JobRoleResponse
                {
                    JobRole_ID = r.JobRole_ID,
                    Role_Name = r.Role_Name,
                    Role_Description = r.Role_Description
                }) : Results.NotFound());
            endpoints.MapPost("/api/jobroles", async (HR.Models.JobRoleRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var jobRole = new JobRole
                {
                    Role_Name = reqModel.Role_Name,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.JobRoles.Add(jobRole);
                await db.SaveChangesAsync();
                return Results.Created($"/api/jobroles/{jobRole.JobRole_ID}", new HR.Models.JobRoleResponse
                {
                    JobRole_ID = jobRole.JobRole_ID,
                    Role_Name = jobRole.Role_Name,
                    Role_Description = jobRole.Role_Description
                });
            });
            endpoints.MapPut("/api/jobroles/{id}", async (int id, HR.Models.JobRoleRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var jobRole = await db.JobRoles.FindAsync(id);
                if (jobRole is null) return Results.NotFound();
                jobRole.Role_Name = reqModel.Role_Name;
                jobRole.UpdatedAt = DateTime.UtcNow;
                jobRole.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(new HR.Models.JobRoleResponse
                {
                    JobRole_ID = jobRole.JobRole_ID,
                    Role_Name = jobRole.Role_Name,
                    Role_Description = jobRole.Role_Description
                });
            });
            endpoints.MapDelete("/api/jobroles/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var jobRole = await db.JobRoles.FindAsync(id);
                if (jobRole is null) return Results.NotFound();
                jobRole.IsDeleted = true;
                jobRole.UpdatedAt = DateTime.UtcNow;
                jobRole.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
