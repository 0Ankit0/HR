using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using HR.Data;
using HR.Models;

namespace HR.Api
{
    public class PersonalGoalApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/personalgoals", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.PersonalGoals.AsQueryable();
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(g => g.Employee_ID == eid);
                var list = await query.ToListAsync();
                return Results.Ok(list.Select(g => new PersonalGoalResponse
                {
                    PersonalGoal_ID = g.PersonalGoal_ID,
                    Employee_ID = g.Employee_ID,
                    Goal = g.Goal,
                    StartDate = g.TargetDate,
                    EndDate = g.TargetDate,
                    IsCompleted = g.IsAchieved
                }));
            });
            endpoints.MapGet("/api/personalgoals/{id}", async (int id, AuthDbContext db) =>
                await db.PersonalGoals.FindAsync(id) is PersonalGoal g ?
                    Results.Ok(new PersonalGoalResponse
                    {
                        PersonalGoal_ID = g.PersonalGoal_ID,
                        Employee_ID = g.Employee_ID,
                        Goal = g.Goal,
                        StartDate = g.TargetDate,
                        EndDate = g.TargetDate,
                        IsCompleted = g.IsAchieved
                    }) : Results.NotFound());
            endpoints.MapPost("/api/personalgoals", async (PersonalGoalRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var goal = new PersonalGoal
                {
                    Employee_ID = reqModel.Employee_ID,
                    Goal = reqModel.Goal,
                    TargetDate = (DateTime)reqModel.EndDate,
                    IsAchieved = false
                };
                db.PersonalGoals.Add(goal);
                await db.SaveChangesAsync();
                return Results.Created($"/api/personalgoals/{goal.PersonalGoal_ID}", new PersonalGoalResponse
                {
                    PersonalGoal_ID = goal.PersonalGoal_ID,
                    Employee_ID = goal.Employee_ID,
                    Goal = goal.Goal,
                    StartDate = goal.TargetDate,
                    EndDate = goal.TargetDate,
                    IsCompleted = goal.IsAchieved
                });
            });
            endpoints.MapPut("/api/personalgoals/{id}", async (int id, PersonalGoalRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var goal = await db.PersonalGoals.FindAsync(id);
                if (goal is null) return Results.NotFound();
                goal.Employee_ID = reqModel.Employee_ID;
                goal.Goal = reqModel.Goal;
                goal.TargetDate = reqModel.EndDate ?? DateTime.MinValue;
                await db.SaveChangesAsync();
                return Results.Ok(new PersonalGoalResponse
                {
                    PersonalGoal_ID = goal.PersonalGoal_ID,
                    Employee_ID = goal.Employee_ID,
                    Goal = goal.Goal,
                    StartDate = goal.TargetDate,
                    EndDate = goal.TargetDate,
                    IsCompleted = goal.IsAchieved
                });
            });
            endpoints.MapDelete("/api/personalgoals/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
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
