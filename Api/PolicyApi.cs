using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class PolicyApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/policies", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Policies.Where(p => !p.IsDeleted);
                // Search by title
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(p => p.Title.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/policies/{id}", async (int id, AuthDbContext db) =>
                await db.Policies.FindAsync(id) is Policy p ? Results.Ok(p) : Results.NotFound());

            endpoints.MapPost("/api/policies", async (Policy p, AuthDbContext db, HttpContext ctx) =>
            {
                p.CreatedAt = DateTime.UtcNow;
                p.CreatedBy = ctx.User?.Identity?.Name;
                db.Policies.Add(p);
                await db.SaveChangesAsync();
                return Results.Created($"/api/policies/{p.Policy_ID}", p);
            });
            endpoints.MapPut("/api/policies/{id}", async (int id, Policy updated, AuthDbContext db, HttpContext ctx) =>
            {
                var p = await db.Policies.FindAsync(id);
                if (p is null) return Results.NotFound();
                p.Title = updated.Title;
                p.Content = updated.Content;
                p.EffectiveDate = updated.EffectiveDate;
                p.UpdatedAt = DateTime.UtcNow;
                p.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(p);
            });
            endpoints.MapDelete("/api/policies/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var p = await db.Policies.FindAsync(id);
                if (p is null) return Results.NotFound();
                p.IsDeleted = true;
                p.UpdatedAt = DateTime.UtcNow;
                p.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}