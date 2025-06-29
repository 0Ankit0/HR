using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class WellnessProgramApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/wellnessprograms", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.WellnessPrograms.Where(w => !w.IsDeleted);
                // Search by title
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(w => w.Title.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/wellnessprograms/{id}", async (int id, AuthDbContext db) =>
                await db.WellnessPrograms.FindAsync(id) is WellnessProgram w ? Results.Ok(w) : Results.NotFound());

            endpoints.MapPost("/api/wellnessprograms", async (WellnessProgram w, AuthDbContext db, HttpContext ctx) =>
            {
                w.CreatedAt = DateTime.UtcNow;
                w.CreatedBy = ctx.User?.Identity?.Name;
                db.WellnessPrograms.Add(w);
                await db.SaveChangesAsync();
                return Results.Created($"/api/wellnessprograms/{w.WellnessProgram_ID}", w);
            });
            endpoints.MapPut("/api/wellnessprograms/{id}", async (int id, WellnessProgram updated, AuthDbContext db, HttpContext ctx) =>
            {
                var w = await db.WellnessPrograms.FindAsync(id);
                if (w is null) return Results.NotFound();
                w.Title = updated.Title;
                w.Description = updated.Description;
                w.StartDate = updated.StartDate;
                w.EndDate = updated.EndDate;
                w.UpdatedAt = DateTime.UtcNow;
                w.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(w);
            });
            endpoints.MapDelete("/api/wellnessprograms/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var w = await db.WellnessPrograms.FindAsync(id);
                if (w is null) return Results.NotFound();
                w.IsDeleted = true;
                w.UpdatedAt = DateTime.UtcNow;
                w.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}