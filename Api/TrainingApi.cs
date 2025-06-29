using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class TrainingApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/trainings", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Trainings.Where(t => !t.IsDeleted);
                // Search by title
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(t => t.Title.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/trainings/{id}", async (int id, AuthDbContext db) =>
                await db.Trainings.FindAsync(id) is Training t ? Results.Ok(t) : Results.NotFound());

            endpoints.MapPost("/api/trainings", async (Training training, AuthDbContext db, HttpContext ctx) =>
            {
                training.CreatedAt = DateTime.UtcNow;
                training.CreatedBy = ctx.User?.Identity?.Name;
                db.Trainings.Add(training);
                await db.SaveChangesAsync();
                return Results.Created($"/api/trainings/{training.Training_ID}", training);
            });
            endpoints.MapPut("/api/trainings/{id}", async (int id, Training updated, AuthDbContext db, HttpContext ctx) =>
            {
                var training = await db.Trainings.FindAsync(id);
                if (training is null) return Results.NotFound();
                training.Title = updated.Title;
                training.Date = updated.Date;
                training.UpdatedAt = DateTime.UtcNow;
                training.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(training);
            });
            endpoints.MapDelete("/api/trainings/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var training = await db.Trainings.FindAsync(id);
                if (training is null) return Results.NotFound();
                training.IsDeleted = true;
                training.UpdatedAt = DateTime.UtcNow;
                training.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
