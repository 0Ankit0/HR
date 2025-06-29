using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;

namespace HR.Api
{
    public class InterviewApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/interviews");

            // Enhanced Interview list: filter, search, paging, exclude deleted
            group.MapGet("/schedule", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Interviews.Include(i => i.Application).Where(i => !i.IsDeleted);
                // Filtering by status
                if (req.Query.TryGetValue("status", out var status) && !string.IsNullOrEmpty(status))
                    query = query.Where(i => i.Status == status);
                // Search by interviewer
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(i => i.Interviewer.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(i => new HR.Models.InterviewResponse
                    {
                        Interview_ID = i.Interview_ID,
                        Application_ID = i.Application_ID,
                        ScheduledAt = i.ScheduledAt,
                        Interviewer = i.Interviewer,
                        Status = i.Status
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            group.MapGet("/schedule/{id}", async (int id, AuthDbContext db) =>
                await db.Interviews.Include(i => i.Application).FirstOrDefaultAsync(i => i.Interview_ID == id && !i.IsDeleted) is Interview interview
                    ? Results.Ok(new HR.Models.InterviewResponse
                    {
                        Interview_ID = interview.Interview_ID,
                        Application_ID = interview.Application_ID,
                        ScheduledAt = interview.ScheduledAt,
                        Interviewer = interview.Interviewer,
                        Status = interview.Status
                    })
                    : Results.NotFound());
            group.MapPost("/schedule", async (HR.Models.InterviewRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var interview = new Interview
                {
                    Application_ID = reqModel.Application_ID,
                    ScheduledAt = reqModel.ScheduledAt,
                    Interviewer = reqModel.Interviewer,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Interviews.Add(interview);
                await db.SaveChangesAsync();
                return Results.Created($"/api/interviews/schedule/{interview.Interview_ID}", new HR.Models.InterviewResponse
                {
                    Interview_ID = interview.Interview_ID,
                    Application_ID = interview.Application_ID,
                    ScheduledAt = interview.ScheduledAt,
                    Interviewer = interview.Interviewer,
                    Status = interview.Status
                });
            });
            group.MapPut("/schedule/{id}", async (int id, HR.Models.InterviewRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var interview = await db.Interviews.FindAsync(id);
                if (interview is null) return Results.NotFound();
                interview.Application_ID = reqModel.Application_ID;
                interview.ScheduledAt = reqModel.ScheduledAt;
                interview.Interviewer = reqModel.Interviewer;
                interview.UpdatedAt = DateTime.UtcNow;
                interview.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(new HR.Models.InterviewResponse
                {
                    Interview_ID = interview.Interview_ID,
                    Application_ID = interview.Application_ID,
                    ScheduledAt = interview.ScheduledAt,
                    Interviewer = interview.Interviewer,
                    Status = interview.Status
                });
            });
            group.MapDelete("/schedule/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
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
