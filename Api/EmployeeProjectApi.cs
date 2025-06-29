using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

namespace HR.Api
{
    public class EmployeeProjectApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/employeeprojects", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Employee_Projects.Where(ep => !ep.IsDeleted);
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(ep => ep.Employee_ID == eid);
                // Filtering by project
                if (req.Query.TryGetValue("projectId", out var pid) && int.TryParse(pid, out var pidInt))
                    query = query.Where(ep => ep.Project_ID == pidInt);
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(ep => new EmployeeProjectResponse
                    {
                        Employee_Project_ID = ep.Employee_Project_ID,
                        Employee_ID = ep.Employee_ID,
                        Project_ID = ep.Project_ID,
                        Assignment_Date = ep.Assignment_Date,
                        Role_on_Project = ep.Role_on_Project
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/employeeprojects/{id}", async (int id, AuthDbContext db) =>
                await db.Employee_Projects.FindAsync(id) is Employee_Project ep ?
                    Results.Ok(new EmployeeProjectResponse
                    {
                        Employee_Project_ID = ep.Employee_Project_ID,
                        Employee_ID = ep.Employee_ID,
                        Project_ID = ep.Project_ID,
                        Assignment_Date = ep.Assignment_Date,
                        Role_on_Project = ep.Role_on_Project
                    }) : Results.NotFound());

            endpoints.MapPost("/api/employeeprojects", async (EmployeeProjectRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var ep = new Employee_Project
                {
                    Employee_ID = reqModel.Employee_ID,
                    Project_ID = reqModel.Project_ID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Employee_Projects.Add(ep);
                await db.SaveChangesAsync();
                var response = new EmployeeProjectResponse
                {
                    Employee_Project_ID = ep.Employee_Project_ID,
                    Employee_ID = ep.Employee_ID,
                    Project_ID = ep.Project_ID,
                    Assignment_Date = ep.Assignment_Date,
                    Role_on_Project = ep.Role_on_Project
                };
                return Results.Created($"/api/employeeprojects/{ep.Employee_Project_ID}", response);
            });
            endpoints.MapPut("/api/employeeprojects/{id}", async (int id, EmployeeProjectRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var ep = await db.Employee_Projects.FindAsync(id);
                if (ep is null) return Results.NotFound();
                ep.Employee_ID = reqModel.Employee_ID;
                ep.Project_ID = reqModel.Project_ID;
                ep.UpdatedAt = DateTime.UtcNow;
                ep.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new EmployeeProjectResponse
                {
                    Employee_Project_ID = ep.Employee_Project_ID,
                    Employee_ID = ep.Employee_ID,
                    Project_ID = ep.Project_ID,
                    Assignment_Date = ep.Assignment_Date,
                    Role_on_Project = ep.Role_on_Project
                };
                return Results.Ok(response);
            });
            endpoints.MapDelete("/api/employeeprojects/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var ep = await db.Employee_Projects.FindAsync(id);
                if (ep is null) return Results.NotFound();
                ep.IsDeleted = true;
                ep.UpdatedAt = DateTime.UtcNow;
                ep.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}