using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using HR.Data;

namespace HR.Api
{
    public class FeedbackApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/feedback");

            // Enhanced Feedback list: filter, search, paging, exclude deleted
            group.MapGet("/continuous", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Feedbacks.Include(f => f.Employee).Where(f => !f.IsDeleted);
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(f => f.Employee_ID == eid);
                // Search by content
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(f => f.Content.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            group.MapGet("/continuous/{id}", async (int id, AuthDbContext db) =>
                await db.Feedbacks.Include(f => f.Employee).FirstOrDefaultAsync(f => f.Feedback_ID == id && !f.IsDeleted) is Feedback feedback
                    ? Results.Ok(feedback)
                    : Results.NotFound());
            group.MapPost("/continuous", async (Feedback feedback, AuthDbContext db, HttpContext ctx) =>
            {
                feedback.CreatedAt = DateTime.UtcNow;
                feedback.CreatedBy = ctx.User?.Identity?.Name;
                db.Feedbacks.Add(feedback);
                await db.SaveChangesAsync();
                return Results.Created($"/api/feedback/continuous/{feedback.Feedback_ID}", feedback);
            });
            group.MapPut("/continuous/{id}", async (int id, Feedback updated, AuthDbContext db, HttpContext ctx) =>
            {
                var feedback = await db.Feedbacks.FindAsync(id);
                if (feedback is null) return Results.NotFound();
                feedback.Content = updated.Content;
                feedback.DateGiven = updated.DateGiven;
                feedback.GivenBy = updated.GivenBy;
                feedback.Employee_ID = updated.Employee_ID;
                feedback.UpdatedAt = DateTime.UtcNow;
                feedback.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(feedback);
            });
            group.MapDelete("/continuous/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var feedback = await db.Feedbacks.FindAsync(id);
                if (feedback is null) return Results.NotFound();
                feedback.IsDeleted = true;
                feedback.UpdatedAt = DateTime.UtcNow;
                feedback.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Enhanced Survey list: filter, search, paging, exclude deleted
            group.MapGet("/surveys", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Surveys.Where(s => !s.IsDeleted);
                // Search by title
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(s => s.Title.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            group.MapGet("/surveys/{id}", async (int id, AuthDbContext db) =>
                await db.Surveys.FindAsync(id) is Survey survey && !survey.IsDeleted
                    ? Results.Ok(survey)
                    : Results.NotFound());
            group.MapPost("/surveys", async (Survey survey, AuthDbContext db, HttpContext ctx) =>
            {
                survey.CreatedAt = DateTime.UtcNow;
                survey.CreatedBy = ctx.User?.Identity?.Name;
                db.Surveys.Add(survey);
                await db.SaveChangesAsync();
                return Results.Created($"/api/feedback/surveys/{survey.Survey_ID}", survey);
            });
            group.MapPut("/surveys/{id}", async (int id, Survey updated, AuthDbContext db, HttpContext ctx) =>
            {
                var survey = await db.Surveys.FindAsync(id);
                if (survey is null) return Results.NotFound();
                survey.Title = updated.Title;
                survey.Description = updated.Description;
                survey.DateCreated = updated.DateCreated;
                survey.UpdatedAt = DateTime.UtcNow;
                survey.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(survey);
            });
            group.MapDelete("/surveys/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var survey = await db.Surveys.FindAsync(id);
                if (survey is null) return Results.NotFound();
                survey.IsDeleted = true;
                survey.UpdatedAt = DateTime.UtcNow;
                survey.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
