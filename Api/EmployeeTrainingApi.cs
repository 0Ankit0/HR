using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

namespace HR.Api
{
    public class EmployeeTrainingApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/employeetrainings", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Employee_Trainings.Where(et => !et.IsDeleted);
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(et => et.Employee_ID == eid);
                // Filtering by training
                if (req.Query.TryGetValue("trainingId", out var tid) && int.TryParse(tid, out var tidInt))
                    query = query.Where(et => et.Training_ID == tidInt);
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(et => new EmployeeTrainingResponse
                    {
                        Employee_Training_ID = et.Employee_Training_ID,
                        Employee_ID = et.Employee_ID,
                        Training_ID = et.Training_ID,
                        Completion_Date = et.Completion_Date,
                        Score = et.Score
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/employeetrainings/{id}", async (int id, AuthDbContext db) =>
                await db.Employee_Trainings.FindAsync(id) is Employee_Training et ?
                    Results.Ok(new EmployeeTrainingResponse
                    {
                        Employee_Training_ID = et.Employee_Training_ID,
                        Employee_ID = et.Employee_ID,
                        Training_ID = et.Training_ID,
                        Completion_Date = et.Completion_Date,
                        Score = et.Score
                    }) : Results.NotFound());

            endpoints.MapPost("/api/employeetrainings", async (EmployeeTrainingRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var et = new Employee_Training
                {
                    Employee_ID = reqModel.Employee_ID,
                    Training_ID = reqModel.Training_ID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Employee_Trainings.Add(et);
                await db.SaveChangesAsync();
                var response = new EmployeeTrainingResponse
                {
                    Employee_Training_ID = et.Employee_Training_ID,
                    Employee_ID = et.Employee_ID,
                    Training_ID = et.Training_ID,
                    Completion_Date = et.Completion_Date,
                    Score = et.Score
                };
                return Results.Created($"/api/employeetrainings/{et.Employee_Training_ID}", response);
            });
            endpoints.MapPut("/api/employeetrainings/{id}", async (int id, EmployeeTrainingRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var et = await db.Employee_Trainings.FindAsync(id);
                if (et is null) return Results.NotFound();
                et.Employee_ID = reqModel.Employee_ID;
                et.Training_ID = reqModel.Training_ID;
                et.UpdatedAt = DateTime.UtcNow;
                et.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new EmployeeTrainingResponse
                {
                    Employee_Training_ID = et.Employee_Training_ID,
                    Employee_ID = et.Employee_ID,
                    Training_ID = et.Training_ID,
                    Completion_Date = et.Completion_Date,
                    Score = et.Score
                };
                return Results.Ok(response);
            });
            endpoints.MapDelete("/api/employeetrainings/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var et = await db.Employee_Trainings.FindAsync(id);
                if (et is null) return Results.NotFound();
                et.IsDeleted = true;
                et.UpdatedAt = DateTime.UtcNow;
                et.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}