using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using HR.Models;

namespace HR.Api
{
    public class InterviewApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/interviews");

            // Get all interviews with filtering and pagination
            group.MapGet("", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Interviews
                    .Include(i => i.Application)
                    .ThenInclude(a => a!.JobPosting)
                    .Where(i => !i.IsDeleted);

                // Filtering by status
                if (req.Query.TryGetValue("status", out var status) && !string.IsNullOrEmpty(status))
                    query = query.Where(i => i.Status == status);

                // Search by interviewer or candidate name
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(i => i.Interviewer.Contains(q!) ||
                                           (i.Application != null && i.Application.CandidateName.Contains(q!)));

                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;

                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(i => new InterviewResponse
                    {
                        Interview_ID = i.Interview_ID,
                        Application_ID = i.Application_ID,
                        CandidateName = i.Application != null ? i.Application.CandidateName : "",
                        CandidateEmail = i.Application != null ? i.Application.CandidateEmail : "",
                        Position = i.Application != null && i.Application.JobPosting != null ? i.Application.JobPosting.Title : "",
                        ScheduledAt = i.ScheduledAt,
                        Interviewer = i.Interviewer,
                        InterviewType = "Video", // Default since not in data model yet
                        Status = i.Status,
                        Notes = i.Feedback // Using Feedback field as Notes
                    }).ToListAsync();

                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            // Get specific interview
            group.MapGet("/{id}", async (int id, AuthDbContext db) =>
            {
                var interview = await db.Interviews
                    .Include(i => i.Application)
                    .ThenInclude(a => a!.JobPosting)
                    .FirstOrDefaultAsync(i => i.Interview_ID == id && !i.IsDeleted);

                if (interview == null) return Results.NotFound();

                return Results.Ok(new InterviewResponse
                {
                    Interview_ID = interview.Interview_ID,
                    Application_ID = interview.Application_ID,
                    CandidateName = interview.Application?.CandidateName ?? "",
                    CandidateEmail = interview.Application?.CandidateEmail ?? "",
                    Position = interview.Application?.JobPosting?.Title ?? "",
                    ScheduledAt = interview.ScheduledAt,
                    Interviewer = interview.Interviewer,
                    InterviewType = "Video", // Default since not in data model yet
                    Status = interview.Status,
                    Notes = interview.Feedback // Using Feedback field as Notes
                });
            });

            // Create new interview
            group.MapPost("", async (InterviewRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var interview = new Interview
                {
                    Application_ID = reqModel.Application_ID,
                    ScheduledAt = reqModel.ScheduledAt,
                    Interviewer = reqModel.Interviewer,
                    Status = "Scheduled",
                    Feedback = reqModel.Notes, // Store Notes in Feedback field
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };

                db.Interviews.Add(interview);
                await db.SaveChangesAsync();

                // Load the application data for response
                await db.Entry(interview)
                    .Reference(i => i.Application)
                    .LoadAsync();
                await db.Entry(interview.Application!)
                    .Reference(a => a.JobPosting)
                    .LoadAsync();

                var response = new InterviewResponse
                {
                    Interview_ID = interview.Interview_ID,
                    Application_ID = interview.Application_ID,
                    CandidateName = interview.Application?.CandidateName ?? reqModel.CandidateName,
                    CandidateEmail = interview.Application?.CandidateEmail ?? reqModel.CandidateEmail,
                    Position = interview.Application?.JobPosting?.Title ?? reqModel.Position,
                    ScheduledAt = interview.ScheduledAt,
                    Interviewer = interview.Interviewer,
                    InterviewType = reqModel.InterviewType,
                    Status = interview.Status,
                    Notes = interview.Feedback
                };

                return Results.Created($"/api/interviews/{interview.Interview_ID}", response);
            });

            // Update interview
            group.MapPut("/{id}", async (int id, InterviewRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var interview = await db.Interviews
                    .Include(i => i.Application)
                    .ThenInclude(a => a!.JobPosting)
                    .FirstOrDefaultAsync(i => i.Interview_ID == id);
                if (interview is null) return Results.NotFound();

                interview.Application_ID = reqModel.Application_ID;
                interview.ScheduledAt = reqModel.ScheduledAt;
                interview.Interviewer = reqModel.Interviewer;
                interview.Feedback = reqModel.Notes; // Store Notes in Feedback field
                interview.UpdatedAt = DateTime.UtcNow;
                interview.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();

                var response = new InterviewResponse
                {
                    Interview_ID = interview.Interview_ID,
                    Application_ID = interview.Application_ID,
                    CandidateName = interview.Application?.CandidateName ?? reqModel.CandidateName,
                    CandidateEmail = interview.Application?.CandidateEmail ?? reqModel.CandidateEmail,
                    Position = interview.Application?.JobPosting?.Title ?? reqModel.Position,
                    ScheduledAt = interview.ScheduledAt,
                    Interviewer = interview.Interviewer,
                    InterviewType = reqModel.InterviewType,
                    Status = interview.Status,
                    Notes = interview.Feedback
                };

                return Results.Ok(response);
            });

            // Update interview status
            group.MapPatch("/{id}/status", async (int id, InterviewStatusUpdateRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var interview = await db.Interviews.FindAsync(id);
                if (interview is null) return Results.NotFound();

                interview.Status = reqModel.Status;
                interview.UpdatedAt = DateTime.UtcNow;
                interview.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.Ok(new { Status = interview.Status });
            });

            // Delete interview
            group.MapDelete("/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var interview = await db.Interviews.FindAsync(id);
                if (interview is null) return Results.NotFound();

                interview.IsDeleted = true;
                interview.UpdatedAt = DateTime.UtcNow;
                interview.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
