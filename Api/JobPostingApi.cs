using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class JobPostingApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/jobpostings", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.JobPostings.Where(jp => !jp.IsDeleted);
                // Search by title
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(jp => jp.Title.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/jobpostings/{id}", async (int id, AuthDbContext db) =>
                await db.JobPostings.FindAsync(id) is JobPosting jp ? Results.Ok(jp) : Results.NotFound());

            endpoints.MapPost("/api/jobpostings", async (JobPosting jp, AuthDbContext db, HttpContext ctx) =>
            {
                jp.CreatedAt = DateTime.UtcNow;
                jp.CreatedBy = ctx.User?.Identity?.Name;
                db.JobPostings.Add(jp);
                await db.SaveChangesAsync();
                return Results.Created($"/api/jobpostings/{jp.JobPosting_ID}", jp);
            });
            endpoints.MapPut("/api/jobpostings/{id}", async (int id, JobPosting updated, AuthDbContext db, HttpContext ctx) =>
            {
                var jp = await db.JobPostings.FindAsync(id);
                if (jp is null) return Results.NotFound();
                jp.Title = updated.Title;
                jp.Description = updated.Description;
                jp.PostedDate = updated.PostedDate;
                jp.EndDate = updated.EndDate;
                jp.IsActive = updated.IsActive;
                jp.UpdatedAt = DateTime.UtcNow;
                jp.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(jp);
            });
            endpoints.MapDelete("/api/jobpostings/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var jp = await db.JobPostings.FindAsync(id);
                if (jp is null) return Results.NotFound();
                jp.IsDeleted = true;
                jp.UpdatedAt = DateTime.UtcNow;
                jp.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}