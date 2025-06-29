using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/employeetrainings/{id}", async (int id, AuthDbContext db) =>
                await db.Employee_Trainings.FindAsync(id) is Employee_Training et ? Results.Ok(et) : Results.NotFound());

            endpoints.MapPost("/api/employeetrainings", async (Employee_Training et, AuthDbContext db, HttpContext ctx) =>
            {
                et.CreatedAt = DateTime.UtcNow;
                et.CreatedBy = ctx.User?.Identity?.Name;
                db.Employee_Trainings.Add(et);
                await db.SaveChangesAsync();
                return Results.Created($"/api/employeetrainings/{et.Employee_Training_ID}", et);
            });
            endpoints.MapPut("/api/employeetrainings/{id}", async (int id, Employee_Training updated, AuthDbContext db, HttpContext ctx) =>
            {
                var et = await db.Employee_Trainings.FindAsync(id);
                if (et is null) return Results.NotFound();
                et.Employee_ID = updated.Employee_ID;
                et.Training_ID = updated.Training_ID;
                et.Completion_Date = updated.Completion_Date;
                et.Score = updated.Score;
                et.UpdatedAt = DateTime.UtcNow;
                et.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(et);
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