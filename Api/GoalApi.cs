using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using HR.Data;
using HR.Models;

namespace HR.Api
{
    public class GoalApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/goals");

            // Enhanced Goals endpoints that match the frontend expectations
            group.MapGet("", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.OKRGoals.Include(g => g.Employee).Where(g => !g.IsDeleted);

                // Filtering
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(g => g.Employee_ID == eid);
                if (req.Query.TryGetValue("type", out var type) && !string.IsNullOrEmpty(type))
                    query = query.Where(g => g.Objective.Contains(type!)); // Using objective field for type filtering
                if (req.Query.TryGetValue("status", out var status) && !string.IsNullOrEmpty(status))
                {
                    if (status == "Completed")
                        query = query.Where(g => g.IsCompleted);
                    else if (status == "Not Started")
                        query = query.Where(g => !g.IsCompleted && g.StartDate > DateTime.UtcNow);
                    else if (status == "In Progress")
                        query = query.Where(g => !g.IsCompleted && g.StartDate <= DateTime.UtcNow && g.EndDate >= DateTime.UtcNow);
                }
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(g => g.Objective.Contains(q!) || g.KeyResults.Contains(q!));

                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;

                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                var goalResponses = items.Select(g => new GoalResponse
                {
                    OKRGoal_ID = g.OKRGoal_ID,
                    Employee_ID = g.Employee_ID,
                    EmployeeName = g.Employee?.Name ?? "Unknown",
                    Title = g.Objective, // Using objective as title
                    Description = g.KeyResults, // Using key results as description
                    Objective = g.Objective,
                    KeyResults = g.KeyResults,
                    Type = DetermineGoalType(g),
                    StartDate = g.StartDate,
                    EndDate = g.EndDate,
                    Progress = CalculateProgress(g),
                    Status = DetermineStatus(g),
                    IsCompleted = g.IsCompleted,
                    CreatedAt = g.CreatedAt
                }).ToList();

                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = goalResponses });
            });

            group.MapGet("/{id}", async (int id, AuthDbContext db) =>
            {
                var goal = await db.OKRGoals.Include(g => g.Employee)
                    .FirstOrDefaultAsync(g => g.OKRGoal_ID == id && !g.IsDeleted);

                if (goal == null) return Results.NotFound();

                var response = new GoalResponse
                {
                    OKRGoal_ID = goal.OKRGoal_ID,
                    Employee_ID = goal.Employee_ID,
                    EmployeeName = goal.Employee?.Name ?? "Unknown",
                    Title = goal.Objective,
                    Description = goal.KeyResults,
                    Objective = goal.Objective,
                    KeyResults = goal.KeyResults,
                    Type = DetermineGoalType(goal),
                    StartDate = goal.StartDate,
                    EndDate = goal.EndDate,
                    Progress = CalculateProgress(goal),
                    Status = DetermineStatus(goal),
                    IsCompleted = goal.IsCompleted,
                    CreatedAt = goal.CreatedAt
                };

                return Results.Ok(response);
            });

            group.MapPost("", async (GoalRequest request, AuthDbContext db, HttpContext ctx) =>
            {
                var goal = new OKRGoal
                {
                    Employee_ID = request.Employee_ID,
                    Objective = !string.IsNullOrEmpty(request.Title) ? request.Title : request.Objective,
                    KeyResults = !string.IsNullOrEmpty(request.Description) ? request.Description : request.KeyResults,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsCompleted = request.Progress >= 100,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };

                db.OKRGoals.Add(goal);
                await db.SaveChangesAsync();

                var response = new GoalResponse
                {
                    OKRGoal_ID = goal.OKRGoal_ID,
                    Employee_ID = goal.Employee_ID,
                    EmployeeName = "Unknown", // Will be populated by a reload
                    Title = goal.Objective,
                    Description = goal.KeyResults,
                    Objective = goal.Objective,
                    KeyResults = goal.KeyResults,
                    Type = request.Type,
                    StartDate = goal.StartDate,
                    EndDate = goal.EndDate,
                    Progress = request.Progress,
                    Status = DetermineStatus(goal),
                    IsCompleted = goal.IsCompleted,
                    CreatedAt = goal.CreatedAt
                };

                return Results.Created($"/api/goals/{goal.OKRGoal_ID}", response);
            });

            group.MapPut("/{id}", async (int id, GoalRequest request, AuthDbContext db, HttpContext ctx) =>
            {
                var goal = await db.OKRGoals.FindAsync(id);
                if (goal == null || goal.IsDeleted) return Results.NotFound();

                goal.Employee_ID = request.Employee_ID;
                goal.Objective = !string.IsNullOrEmpty(request.Title) ? request.Title : request.Objective;
                goal.KeyResults = !string.IsNullOrEmpty(request.Description) ? request.Description : request.KeyResults;
                goal.StartDate = request.StartDate;
                goal.EndDate = request.EndDate;
                goal.IsCompleted = request.Progress >= 100;
                goal.UpdatedAt = DateTime.UtcNow;
                goal.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();

                var response = new GoalResponse
                {
                    OKRGoal_ID = goal.OKRGoal_ID,
                    Employee_ID = goal.Employee_ID,
                    EmployeeName = "Unknown",
                    Title = goal.Objective,
                    Description = goal.KeyResults,
                    Objective = goal.Objective,
                    KeyResults = goal.KeyResults,
                    Type = request.Type,
                    StartDate = goal.StartDate,
                    EndDate = goal.EndDate,
                    Progress = request.Progress,
                    Status = DetermineStatus(goal),
                    IsCompleted = goal.IsCompleted,
                    CreatedAt = goal.CreatedAt
                };

                return Results.Ok(response);
            });

            group.MapPatch("/{id}/status", async (int id, GoalStatusUpdateRequest request, AuthDbContext db, HttpContext ctx) =>
            {
                var goal = await db.OKRGoals.FindAsync(id);
                if (goal == null || goal.IsDeleted) return Results.NotFound();

                goal.IsCompleted = request.IsCompleted || request.Progress >= 100;
                goal.UpdatedAt = DateTime.UtcNow;
                goal.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.Ok(new { Message = "Goal status updated successfully" });
            });

            group.MapDelete("/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var goal = await db.OKRGoals.FindAsync(id);
                if (goal == null || goal.IsDeleted) return Results.NotFound();

                goal.IsDeleted = true;
                goal.UpdatedAt = DateTime.UtcNow;
                goal.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Stats endpoint
            group.MapGet("/stats", async (AuthDbContext db) =>
            {
                var goals = await db.OKRGoals.Where(g => !g.IsDeleted).ToListAsync();
                var totalGoals = goals.Count;
                var completedGoals = goals.Count(g => g.IsCompleted);
                var activeGoals = goals.Count(g => !g.IsCompleted && g.StartDate <= DateTime.UtcNow && g.EndDate >= DateTime.UtcNow);
                var upcomingGoals = goals.Count(g => g.StartDate > DateTime.UtcNow);

                return Results.Ok(new
                {
                    TotalGoals = totalGoals,
                    CompletedGoals = completedGoals,
                    ActiveGoals = activeGoals,
                    UpcomingGoals = upcomingGoals,
                    CompletionRate = totalGoals > 0 ? (int)((double)completedGoals / totalGoals * 100) : 0
                });
            });
        }

        private static string DetermineGoalType(OKRGoal goal)
        {
            // Simple logic to determine type based on objective content
            var objective = goal.Objective.ToLower();
            if (objective.Contains("team") || objective.Contains("department"))
                return "Team";
            if (objective.Contains("company") || objective.Contains("organization"))
                return "Company";
            return "Personal";
        }

        private static int CalculateProgress(OKRGoal goal)
        {
            if (goal.IsCompleted) return 100;

            var totalDays = (goal.EndDate - goal.StartDate).TotalDays;
            var elapsedDays = (DateTime.UtcNow - goal.StartDate).TotalDays;

            if (elapsedDays <= 0) return 0;
            if (elapsedDays >= totalDays) return goal.IsCompleted ? 100 : 90;

            return (int)(elapsedDays / totalDays * 80); // Assuming 80% progress if not completed but within timeframe
        }

        private static string DetermineStatus(OKRGoal goal)
        {
            if (goal.IsCompleted) return "Completed";
            if (goal.StartDate > DateTime.UtcNow) return "Not Started";
            if (goal.EndDate < DateTime.UtcNow) return "Overdue";
            return "In Progress";
        }
    }
}
