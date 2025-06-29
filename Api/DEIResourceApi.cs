using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class DEIResourceApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/deiresources", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.DEIResources.Where(d => !d.IsDeleted);
                // Search by title
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(d => d.Title.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/deiresources/{id}", async (int id, AuthDbContext db) =>
                await db.DEIResources.FindAsync(id) is DEIResource d ? Results.Ok(d) : Results.NotFound());

            endpoints.MapPost("/api/deiresources", async (DEIResource d, AuthDbContext db, HttpContext ctx) =>
            {
                d.CreatedAt = DateTime.UtcNow;
                d.CreatedBy = ctx.User?.Identity?.Name;
                db.DEIResources.Add(d);
                await db.SaveChangesAsync();
                return Results.Created($"/api/deiresources/{d.DEIResource_ID}", d);
            });
            endpoints.MapPut("/api/deiresources/{id}", async (int id, DEIResource updated, AuthDbContext db, HttpContext ctx) =>
            {
                var d = await db.DEIResources.FindAsync(id);
                if (d is null) return Results.NotFound();
                d.Title = updated.Title;
                d.Content = updated.Content;
                d.UpdatedAt = DateTime.UtcNow;
                d.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(d);
            });
            endpoints.MapDelete("/api/deiresources/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var d = await db.DEIResources.FindAsync(id);
                if (d is null) return Results.NotFound();
                d.IsDeleted = true;
                d.UpdatedAt = DateTime.UtcNow;
                d.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}