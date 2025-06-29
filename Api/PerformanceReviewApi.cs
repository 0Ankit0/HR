using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(pr => new PerformanceReviewResponse
                    {
                        PerformanceReview_ID = pr.PerformanceReview_ID,
                        Employee_ID = pr.Employee_ID,
                        Reviewer = pr.Reviewer,
                        Comments = pr.Comments,
                        ReviewDate = pr.ReviewDate,
                        Score = pr.Score
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            endpoints.MapGet("/api/performancereviews/{id}", async (int id, AuthDbContext db) =>
                await db.PerformanceReviews.FindAsync(id) is PerformanceReview pr ?
                    Results.Ok(new PerformanceReviewResponse
                    {
                        PerformanceReview_ID = pr.PerformanceReview_ID,
                        Employee_ID = pr.Employee_ID,
                        Reviewer = pr.Reviewer,
                        Comments = pr.Comments,
                        ReviewDate = pr.ReviewDate,
                        Score = pr.Score
                    }) : Results.NotFound());
            endpoints.MapPost("/api/performancereviews", async (PerformanceReviewRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var review = new PerformanceReview
                {
                    Employee_ID = reqModel.Employee_ID,
                    Reviewer = reqModel.Reviewer,
                    Comments = reqModel.Comments,
                    ReviewDate = reqModel.ReviewDate,
                    Score = reqModel.Score
                };
                db.PerformanceReviews.Add(review);
                await db.SaveChangesAsync();
                return Results.Created($"/api/performancereviews/{review.PerformanceReview_ID}", new PerformanceReviewResponse
                {
                    PerformanceReview_ID = review.PerformanceReview_ID,
                    Employee_ID = review.Employee_ID,
                    Reviewer = review.Reviewer,
                    Comments = review.Comments,
                    ReviewDate = review.ReviewDate,
                    Score = review.Score
                });
            });
            endpoints.MapPut("/api/performancereviews/{id}", async (int id, PerformanceReviewRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var pr = await db.PerformanceReviews.FindAsync(id);
                if (pr is null) return Results.NotFound();
                pr.Employee_ID = reqModel.Employee_ID;
                pr.Reviewer = reqModel.Reviewer;
                pr.Comments = reqModel.Comments;
                pr.ReviewDate = reqModel.ReviewDate;
                pr.Score = reqModel.Score;
                await db.SaveChangesAsync();
                return Results.Ok(new PerformanceReviewResponse
                {
                    PerformanceReview_ID = pr.PerformanceReview_ID,
                    Employee_ID = pr.Employee_ID,
                    Reviewer = pr.Reviewer,
                    Comments = pr.Comments,
                    ReviewDate = pr.ReviewDate,
                    Score = pr.Score
                });
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