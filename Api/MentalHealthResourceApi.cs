using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class MentalHealthResourceApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/mentalhealthresources", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.MentalHealthResources.Where(m => !m.IsDeleted);
                // Search by title
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(m => m.Title.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/mentalhealthresources/{id}", async (int id, AuthDbContext db) =>
                await db.MentalHealthResources.FindAsync(id) is MentalHealthResource m ? Results.Ok(m) : Results.NotFound());

            endpoints.MapPost("/api/mentalhealthresources", async (MentalHealthResource m, AuthDbContext db, HttpContext ctx) =>
            {
                m.CreatedAt = DateTime.UtcNow;
                m.CreatedBy = ctx.User?.Identity?.Name;
                db.MentalHealthResources.Add(m);
                await db.SaveChangesAsync();
                return Results.Created($"/api/mentalhealthresources/{m.MentalHealthResource_ID}", m);
            });
            endpoints.MapPut("/api/mentalhealthresources/{id}", async (int id, MentalHealthResource updated, AuthDbContext db, HttpContext ctx) =>
            {
                var m = await db.MentalHealthResources.FindAsync(id);
                if (m is null) return Results.NotFound();
                m.Title = updated.Title;
                m.Description = updated.Description;
                m.ContactInfo = updated.ContactInfo;
                m.UpdatedAt = DateTime.UtcNow;
                m.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(m);
            });
            endpoints.MapDelete("/api/mentalhealthresources/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var m = await db.MentalHealthResources.FindAsync(id);
                if (m is null) return Results.NotFound();
                m.IsDeleted = true;
                m.UpdatedAt = DateTime.UtcNow;
                m.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}