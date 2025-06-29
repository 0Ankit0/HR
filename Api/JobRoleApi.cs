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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/jobroles/{id}", async (int id, AuthDbContext db) =>
                await db.JobRoles.FindAsync(id) is JobRole r ? Results.Ok(r) : Results.NotFound());

            endpoints.MapPost("/api/jobroles", async (JobRole jobRole, AuthDbContext db, HttpContext ctx) =>
            {
                jobRole.CreatedAt = DateTime.UtcNow;
                jobRole.CreatedBy = ctx.User?.Identity?.Name;
                db.JobRoles.Add(jobRole);
                await db.SaveChangesAsync();
                return Results.Created($"/api/jobroles/{jobRole.JobRole_ID}", jobRole);
            });
            endpoints.MapPut("/api/jobroles/{id}", async (int id, JobRole updated, AuthDbContext db, HttpContext ctx) =>
            {
                var jobRole = await db.JobRoles.FindAsync(id);
                if (jobRole is null) return Results.NotFound();
                jobRole.Role_Name = updated.Role_Name;
                jobRole.Role_Description = updated.Role_Description;
                jobRole.UpdatedAt = DateTime.UtcNow;
                jobRole.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(jobRole);
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
