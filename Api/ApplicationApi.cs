using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;
using Application = HR.Data.Application;

namespace HR.Api
{
    public class ApplicationApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/applications", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Applications.Where(a => !a.IsDeleted);
                // Filtering by job posting
                if (req.Query.TryGetValue("jobPostingId", out var jpId) && int.TryParse(jpId, out var jid))
                    query = query.Where(a => a.JobPosting_ID == jid);
                // Search by candidate name
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(a => a.CandidateName.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(a => new ApplicationResponse
                    {
                        Application_ID = a.Application_ID,
                        JobPosting_ID = a.JobPosting_ID,
                        CandidateName = a.CandidateName,
                        CandidateEmail = a.CandidateEmail,
                        AppliedDate = a.AppliedDate,
                        Status = a.Status
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/applications/{id}", async (int id, AuthDbContext db) =>
                await db.Applications.FindAsync(id) is Application a ?
                    Results.Ok(new ApplicationResponse
                    {
                        Application_ID = a.Application_ID,
                        JobPosting_ID = a.JobPosting_ID,
                        CandidateName = a.CandidateName,
                        CandidateEmail = a.CandidateEmail,
                        AppliedDate = a.AppliedDate,
                        Status = a.Status
                    }) : Results.NotFound());

            endpoints.MapPost("/api/applications", async (ApplicationRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var a = new Application
                {
                    JobPosting_ID = reqModel.JobPosting_ID,
                    CandidateName = reqModel.CandidateName,
                    CandidateEmail = reqModel.CandidateEmail,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Applications.Add(a);
                await db.SaveChangesAsync();
                var response = new ApplicationResponse
                {
                    Application_ID = a.Application_ID,
                    JobPosting_ID = a.JobPosting_ID,
                    CandidateName = a.CandidateName,
                    CandidateEmail = a.CandidateEmail,
                    AppliedDate = a.AppliedDate,
                    Status = a.Status
                };
                return Results.Created($"/api/applications/{a.Application_ID}", response);
            });

            endpoints.MapPut("/api/applications/{id}", async (int id, ApplicationRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var a = await db.Applications.FindAsync(id);
                if (a is null) return Results.NotFound();
                a.JobPosting_ID = reqModel.JobPosting_ID;
                a.CandidateName = reqModel.CandidateName;
                a.CandidateEmail = reqModel.CandidateEmail;
                a.UpdatedAt = DateTime.UtcNow;
                a.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new ApplicationResponse
                {
                    Application_ID = a.Application_ID,
                    JobPosting_ID = a.JobPosting_ID,
                    CandidateName = a.CandidateName,
                    CandidateEmail = a.CandidateEmail,
                    AppliedDate = a.AppliedDate,
                    Status = a.Status
                };
                return Results.Ok(response);
            });
            endpoints.MapDelete("/api/applications/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var a = await db.Applications.FindAsync(id);
                if (a is null) return Results.NotFound();
                a.IsDeleted = true;
                a.UpdatedAt = DateTime.UtcNow;
                a.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}