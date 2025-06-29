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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            group.MapGet("/schedule/{id}", async (int id, AuthDbContext db) =>
                await db.Interviews.Include(i => i.Application).FirstOrDefaultAsync(i => i.Interview_ID == id && !i.IsDeleted) is Interview interview
                    ? Results.Ok(interview)
                    : Results.NotFound());
            group.MapPost("/schedule", async (Interview interview, AuthDbContext db, HttpContext ctx) =>
            {
                interview.CreatedAt = DateTime.UtcNow;
                interview.CreatedBy = ctx.User?.Identity?.Name;
                db.Interviews.Add(interview);
                await db.SaveChangesAsync();
                return Results.Created($"/api/interviews/schedule/{interview.Interview_ID}", interview);
            });
            group.MapPut("/schedule/{id}", async (int id, Interview updated, AuthDbContext db, HttpContext ctx) =>
            {
                var interview = await db.Interviews.FindAsync(id);
                if (interview is null) return Results.NotFound();
                interview.ScheduledAt = updated.ScheduledAt;
                interview.Interviewer = updated.Interviewer;
                interview.Feedback = updated.Feedback;
                interview.Status = updated.Status;
                interview.Application_ID = updated.Application_ID;
                interview.UpdatedAt = DateTime.UtcNow;
                interview.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(interview);
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
