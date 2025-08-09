using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;
using System.Text.Json;

namespace HR.Api
{
    public class PerformanceReviewApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/api/performancereviews");

            // Enhanced list: filter, search, paging, exclude deleted
            group.MapGet("", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.PerformanceReviews.Where(pr => !pr.IsDeleted);

                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(pr => pr.Employee_ID == eid);

                // Date range filtering
                if (req.Query.TryGetValue("startDate", out var startDateStr) && DateTime.TryParse(startDateStr, out var startDate))
                    query = query.Where(pr => pr.ReviewDate >= startDate);

                if (req.Query.TryGetValue("endDate", out var endDateStr) && DateTime.TryParse(endDateStr, out var endDate))
                    query = query.Where(pr => pr.ReviewDate <= endDate);

                // Search by reviewer or employee name
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(pr => pr.Reviewer.Contains(q!) ||
                                              (pr.Employee != null && pr.Employee.Name.Contains(q!)));

                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;

                var total = await query.CountAsync();
                var items = await query
                    .Include(pr => pr.Employee)
                        .ThenInclude(e => e!.Department)
                    .Include(pr => pr.Employee)
                        .ThenInclude(e => e!.JobRole)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(pr => new PerformanceReviewResponse
                    {
                        PerformanceReview_ID = pr.PerformanceReview_ID,
                        Employee_ID = pr.Employee_ID,
                        EmployeeName = pr.Employee != null ? pr.Employee.Name : "",
                        DepartmentName = pr.Employee != null && pr.Employee.Department != null ? pr.Employee.Department.Department_Name : "",
                        JobRoleName = pr.Employee != null && pr.Employee.JobRole != null ? pr.Employee.JobRole.Role_Name : "",
                        Reviewer = pr.Reviewer,
                        Comments = pr.Comments,
                        ReviewDate = pr.ReviewDate,
                        Score = pr.Score,
                        ReviewType = "Annual", // Default since not in current model
                        Status = "Published", // Default since not in current model
                        Goals = "", // Default since not in current model
                        Achievements = "", // Default since not in current model
                        AreasForImprovement = "", // Default since not in current model
                        NextReviewDate = pr.ReviewDate.AddYears(1), // Default logic
                        CreatedDate = pr.CreatedAt,
                        LastModifiedDate = pr.UpdatedAt
                    }).ToListAsync();

                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            // Get single performance review
            group.MapGet("{id}", async (int id, AuthDbContext db) =>
            {
                var pr = await db.PerformanceReviews
                    .Include(pr => pr.Employee)
                        .ThenInclude(e => e!.Department)
                    .Include(pr => pr.Employee)
                        .ThenInclude(e => e!.JobRole)
                    .FirstOrDefaultAsync(pr => pr.PerformanceReview_ID == id && !pr.IsDeleted);

                if (pr == null) return Results.NotFound();

                return Results.Ok(new PerformanceReviewResponse
                {
                    PerformanceReview_ID = pr.PerformanceReview_ID,
                    Employee_ID = pr.Employee_ID,
                    EmployeeName = pr.Employee?.Name ?? "",
                    DepartmentName = pr.Employee?.Department?.Department_Name ?? "",
                    JobRoleName = pr.Employee?.JobRole?.Role_Name ?? "",
                    Reviewer = pr.Reviewer,
                    Comments = pr.Comments,
                    ReviewDate = pr.ReviewDate,
                    Score = pr.Score,
                    ReviewType = "Annual",
                    Status = "Published",
                    Goals = "",
                    Achievements = "",
                    AreasForImprovement = "",
                    NextReviewDate = pr.ReviewDate.AddYears(1),
                    CreatedDate = pr.CreatedAt,
                    LastModifiedDate = pr.UpdatedAt
                });
            });

            // Create performance review
            group.MapPost("", async (PerformanceReviewRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var review = new PerformanceReview
                {
                    Employee_ID = reqModel.Employee_ID,
                    Reviewer = reqModel.Reviewer,
                    Comments = reqModel.Comments,
                    ReviewDate = reqModel.ReviewDate,
                    Score = reqModel.Score,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };

                db.PerformanceReviews.Add(review);
                await db.SaveChangesAsync();
                return Results.Created($"/api/performancereviews/{review.PerformanceReview_ID}", review);
            });

            // Update performance review
            group.MapPut("{id}", async (int id, PerformanceReviewRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var review = await db.PerformanceReviews.FindAsync(id);
                if (review == null || review.IsDeleted) return Results.NotFound();

                review.Employee_ID = reqModel.Employee_ID;
                review.Reviewer = reqModel.Reviewer;
                review.Comments = reqModel.Comments;
                review.ReviewDate = reqModel.ReviewDate;
                review.Score = reqModel.Score;
                review.UpdatedAt = DateTime.UtcNow;
                review.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.Ok(review);
            });

            // Update performance review score
            group.MapPatch("{id}/score", async (int id, JsonElement scoreData, AuthDbContext db, HttpContext ctx) =>
            {
                var review = await db.PerformanceReviews.FindAsync(id);
                if (review == null || review.IsDeleted) return Results.NotFound();

                if (scoreData.TryGetProperty("score", out var scoreElement))
                {
                    review.Score = scoreElement.GetInt32();
                    review.UpdatedAt = DateTime.UtcNow;
                    review.UpdatedBy = ctx.User?.Identity?.Name;
                    await db.SaveChangesAsync();
                    return Results.Ok(new { review.PerformanceReview_ID, review.Score });
                }

                return Results.BadRequest("Score is required");
            });

            // Delete performance review (soft delete)
            group.MapDelete("{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var review = await db.PerformanceReviews.FindAsync(id);
                if (review == null) return Results.NotFound();

                review.IsDeleted = true;
                review.UpdatedAt = DateTime.UtcNow;
                review.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok();
            });

            // Get performance review statistics
            group.MapGet("stats", async (AuthDbContext db) =>
            {
                var totalReviews = await db.PerformanceReviews.CountAsync(pr => !pr.IsDeleted);
                var recentReviews = await db.PerformanceReviews.CountAsync(pr => !pr.IsDeleted && pr.ReviewDate >= DateTime.Now.AddMonths(-3));
                var averageScore = await db.PerformanceReviews
                    .Where(pr => !pr.IsDeleted)
                    .AverageAsync(pr => (double?)pr.Score) ?? 0.0;

                var reviewsByDepartment = await db.PerformanceReviews
                    .Where(pr => !pr.IsDeleted)
                    .Include(pr => pr.Employee!.Department)
                    .Where(pr => pr.Employee != null && pr.Employee.Department != null)
                    .GroupBy(pr => pr.Employee!.Department!.Department_Name)
                    .Select(g => new ReviewsByDepartment
                    {
                        DepartmentName = g.Key,
                        Count = g.Count(),
                        AverageScore = g.Average(pr => pr.Score)
                    })
                    .ToListAsync();

                var reviewsByMonth = await db.PerformanceReviews
                    .Where(pr => !pr.IsDeleted && pr.ReviewDate >= DateTime.Now.AddMonths(-12))
                    .GroupBy(pr => new { pr.ReviewDate.Year, pr.ReviewDate.Month })
                    .Select(g => new ReviewsByMonth
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Count = g.Count(),
                        AverageScore = g.Average(pr => pr.Score)
                    })
                    .ToListAsync();

                var stats = new PerformanceReviewStats
                {
                    TotalReviews = totalReviews,
                    CompletedReviews = totalReviews, // All are considered completed in current model
                    PendingReviews = 0, // No pending status in current model
                    OverdueReviews = recentReviews, // Using recent reviews as placeholder
                    AverageScore = averageScore,
                    ReviewsByDepartment = reviewsByDepartment,
                    ReviewsByMonth = reviewsByMonth
                };

                return Results.Ok(stats);
            });
        }
    }
}