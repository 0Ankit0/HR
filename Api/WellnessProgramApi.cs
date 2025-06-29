using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(w => new WellnessProgramResponse
                    {
                        WellnessProgram_ID = w.WellnessProgram_ID,
                        Title = w.Title,
                        Description = w.Description,
                        StartDate = w.StartDate,
                        EndDate = w.EndDate.HasValue ? w.EndDate.Value : default(DateTime)
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/wellnessprograms/{id}", async (int id, AuthDbContext db) =>
                await db.WellnessPrograms.FindAsync(id) is WellnessProgram w ?
                    Results.Ok(new WellnessProgramResponse
                    {
                        WellnessProgram_ID = w.WellnessProgram_ID,
                        Title = w.Title,
                        Description = w.Description,
                        StartDate = w.StartDate,
                        EndDate = w.EndDate.HasValue ? w.EndDate.Value : default(DateTime)
                    }) : Results.NotFound());

            endpoints.MapPost("/api/wellnessprograms", async (WellnessProgramRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var w = new WellnessProgram
                {
                    Title = reqModel.Title,
                    Description = reqModel.Description,
                    StartDate = reqModel.StartDate,
                    EndDate = reqModel.EndDate
                };
                db.WellnessPrograms.Add(w);
                await db.SaveChangesAsync();
                return Results.Created($"/api/wellnessprograms/{w.WellnessProgram_ID}", new WellnessProgramResponse
                {
                    WellnessProgram_ID = w.WellnessProgram_ID,
                    Title = w.Title,
                    Description = w.Description,
                    StartDate = w.StartDate,
                    EndDate = w.EndDate.HasValue ? w.EndDate.Value : default(DateTime)
                });
            });
            endpoints.MapPut("/api/wellnessprograms/{id}", async (int id, WellnessProgramRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var w = await db.WellnessPrograms.FindAsync(id);
                if (w is null) return Results.NotFound();
                w.Title = reqModel.Title;
                w.Description = reqModel.Description;
                w.StartDate = reqModel.StartDate;
                w.EndDate = reqModel.EndDate;
                await db.SaveChangesAsync();
                return Results.Ok(new WellnessProgramResponse
                {
                    WellnessProgram_ID = w.WellnessProgram_ID,
                    Title = w.Title,
                    Description = w.Description,
                    StartDate = w.StartDate,
                    EndDate = w.EndDate.HasValue ? w.EndDate.Value : default(DateTime)
                });
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