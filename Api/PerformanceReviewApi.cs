using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class PerformanceReviewApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/performancereviews", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.PerformanceReviews.Where(pr => !pr.IsDeleted);
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(pr => pr.Employee_ID == eid);
                // Search by reviewer
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(pr => pr.Reviewer.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/performancereviews/{id}", async (int id, AuthDbContext db) =>
                await db.PerformanceReviews.FindAsync(id) is PerformanceReview pr ? Results.Ok(pr) : Results.NotFound());

            endpoints.MapPost("/api/performancereviews", async (PerformanceReview pr, AuthDbContext db, HttpContext ctx) =>
            {
                pr.CreatedAt = DateTime.UtcNow;
                pr.CreatedBy = ctx.User?.Identity?.Name;
                db.PerformanceReviews.Add(pr);
                await db.SaveChangesAsync();
                return Results.Created($"/api/performancereviews/{pr.PerformanceReview_ID}", pr);
            });
            endpoints.MapPut("/api/performancereviews/{id}", async (int id, PerformanceReview updated, AuthDbContext db, HttpContext ctx) =>
            {
                var pr = await db.PerformanceReviews.FindAsync(id);
                if (pr is null) return Results.NotFound();
                pr.ReviewDate = updated.ReviewDate;
                pr.Reviewer = updated.Reviewer;
                pr.Comments = updated.Comments;
                pr.Score = updated.Score;
                pr.Employee_ID = updated.Employee_ID;
                pr.UpdatedAt = DateTime.UtcNow;
                pr.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(pr);
            });
            endpoints.MapDelete("/api/performancereviews/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var pr = await db.PerformanceReviews.FindAsync(id);
                if (pr is null) return Results.NotFound();
                pr.IsDeleted = true;
                pr.UpdatedAt = DateTime.UtcNow;
                pr.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}