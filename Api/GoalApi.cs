using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using HR.Data;

namespace HR.Api
{
    public class GoalApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/goals");

            // Only keep enhanced OKR Goals endpoints
            group.MapGet("/okr", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.OKRGoals.Include(g => g.Employee).Where(g => !g.IsDeleted);
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(g => g.Employee_ID == eid);
                // Search by objective
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(g => g.Objective.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            group.MapGet("/okr/{id}", async (int id, AuthDbContext db) =>
                await db.OKRGoals.Include(g => g.Employee).FirstOrDefaultAsync(g => g.OKRGoal_ID == id && !g.IsDeleted) is OKRGoal goal
                    ? Results.Ok(goal)
                    : Results.NotFound());
            group.MapPost("/okr", async (OKRGoal goal, AuthDbContext db, HttpContext ctx) =>
            {
                goal.CreatedAt = DateTime.UtcNow;
                goal.CreatedBy = ctx.User?.Identity?.Name;
                db.OKRGoals.Add(goal);
                await db.SaveChangesAsync();
                return Results.Created($"/api/goals/okr/{goal.OKRGoal_ID}", goal);
            });
            group.MapPut("/okr/{id}", async (int id, OKRGoal updated, AuthDbContext db, HttpContext ctx) =>
            {
                var goal = await db.OKRGoals.FindAsync(id);
                if (goal is null) return Results.NotFound();
                goal.Objective = updated.Objective;
                goal.KeyResults = updated.KeyResults;
                goal.StartDate = updated.StartDate;
                goal.EndDate = updated.EndDate;
                goal.IsCompleted = updated.IsCompleted;
                goal.Employee_ID = updated.Employee_ID;
                goal.UpdatedAt = DateTime.UtcNow;
                goal.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(goal);
            });
            group.MapDelete("/okr/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var goal = await db.OKRGoals.FindAsync(id);
                if (goal is null) return Results.NotFound();
                goal.IsDeleted = true;
                goal.UpdatedAt = DateTime.UtcNow;
                goal.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
            // Only keep enhanced Personal Goals endpoints
            group.MapGet("/personal", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.PersonalGoals.AsQueryable();
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(g => g.Employee_ID == eid);
                var list = await query.ToListAsync();
                return Results.Ok(list.Select(g => new HR.Models.PersonalGoalResponse
                {
                    PersonalGoal_ID = g.PersonalGoal_ID,
                    Employee_ID = g.Employee_ID,
                    Goal = g.Goal,
                    StartDate = g.TargetDate,
                    EndDate = g.TargetDate,
                    IsCompleted = g.IsAchieved
                }));
            });
            group.MapGet("/personal/{id}", async (int id, AuthDbContext db) =>
                await db.PersonalGoals.FindAsync(id) is PersonalGoal g ?
                    Results.Ok(new HR.Models.PersonalGoalResponse
                    {
                        PersonalGoal_ID = g.PersonalGoal_ID,
                        Employee_ID = g.Employee_ID,
                        Goal = g.Goal,
                        StartDate = g.TargetDate,
                        EndDate = g.TargetDate,
                        IsCompleted = g.IsAchieved
                    }) : Results.NotFound());
            group.MapPost("/personal", async (HR.Models.PersonalGoalRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var goal = new PersonalGoal
                {
                    Employee_ID = reqModel.Employee_ID,
                    Goal = reqModel.Goal,
                    TargetDate = reqModel.EndDate,
                    IsAchieved = false
                };
                db.PersonalGoals.Add(goal);
                await db.SaveChangesAsync();
                return Results.Created($"/api/goals/personal/{goal.PersonalGoal_ID}", new HR.Models.PersonalGoalResponse
                {
                    PersonalGoal_ID = goal.PersonalGoal_ID,
                    Employee_ID = goal.Employee_ID,
                    Goal = goal.Goal,
                    StartDate = goal.TargetDate,
                    EndDate = goal.TargetDate,
                    IsCompleted = goal.IsAchieved
                });
            });
            group.MapPut("/personal/{id}", async (int id, HR.Models.PersonalGoalRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var goal = await db.PersonalGoals.FindAsync(id);
                if (goal is null) return Results.NotFound();
                goal.Employee_ID = reqModel.Employee_ID;
                goal.Goal = reqModel.Goal;
                goal.TargetDate = reqModel.EndDate;
                await db.SaveChangesAsync();
                return Results.Ok(new HR.Models.PersonalGoalResponse
                {
                    PersonalGoal_ID = goal.PersonalGoal_ID,
                    Employee_ID = goal.Employee_ID,
                    Goal = goal.Goal,
                    StartDate = goal.TargetDate,
                    EndDate = goal.TargetDate,
                    IsCompleted = goal.IsAchieved
                });
            });
            group.MapDelete("/personal/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var goal = await db.PersonalGoals.FindAsync(id);
                if (goal is null) return Results.NotFound();
                db.PersonalGoals.Remove(goal);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
