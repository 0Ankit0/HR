using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(jp => new JobPostingResponse
                    {
                        JobPosting_ID = jp.JobPosting_ID,
                        Title = jp.Title,
                        Description = jp.Description,
                        PostedDate = jp.PostedDate,
                        IsActive = jp.IsActive
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/jobpostings/{id}", async (int id, AuthDbContext db) =>
                await db.JobPostings.FindAsync(id) is JobPosting jp ?
                    Results.Ok(new JobPostingResponse
                    {
                        JobPosting_ID = jp.JobPosting_ID,
                        Title = jp.Title,
                        Description = jp.Description,
                        PostedDate = jp.PostedDate,
                        IsActive = jp.IsActive
                    }) : Results.NotFound());

            endpoints.MapPost("/api/jobpostings", async (JobPostingRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var jp = new JobPosting
                {
                    Title = reqModel.Title,
                    Description = reqModel.Description,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.JobPostings.Add(jp);
                await db.SaveChangesAsync();
                var response = new JobPostingResponse
                {
                    JobPosting_ID = jp.JobPosting_ID,
                    Title = jp.Title,
                    Description = jp.Description,
                    PostedDate = jp.PostedDate,
                    IsActive = jp.IsActive
                };
                return Results.Created($"/api/jobpostings/{jp.JobPosting_ID}", response);
            });
            endpoints.MapPut("/api/jobpostings/{id}", async (int id, JobPostingRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var jp = await db.JobPostings.FindAsync(id);
                if (jp is null) return Results.NotFound();
                jp.Title = reqModel.Title;
                jp.Description = reqModel.Description;
                jp.UpdatedAt = DateTime.UtcNow;
                jp.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new JobPostingResponse
                {
                    JobPosting_ID = jp.JobPosting_ID,
                    Title = jp.Title,
                    Description = jp.Description,
                    PostedDate = jp.PostedDate,
                    IsActive = jp.IsActive
                };
                return Results.Ok(response);
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