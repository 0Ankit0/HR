using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/employeeprojects/{id}", async (int id, AuthDbContext db) =>
                await db.Employee_Projects.FindAsync(id) is Employee_Project ep ? Results.Ok(ep) : Results.NotFound());

            endpoints.MapPost("/api/employeeprojects", async (Employee_Project ep, AuthDbContext db, HttpContext ctx) =>
            {
                ep.CreatedAt = DateTime.UtcNow;
                ep.CreatedBy = ctx.User?.Identity?.Name;
                db.Employee_Projects.Add(ep);
                await db.SaveChangesAsync();
                return Results.Created($"/api/employeeprojects/{ep.Employee_Project_ID}", ep);
            });
            endpoints.MapPut("/api/employeeprojects/{id}", async (int id, Employee_Project updated, AuthDbContext db, HttpContext ctx) =>
            {
                var ep = await db.Employee_Projects.FindAsync(id);
                if (ep is null) return Results.NotFound();
                ep.Employee_ID = updated.Employee_ID;
                ep.Project_ID = updated.Project_ID;
                ep.Assignment_Date = updated.Assignment_Date;
                ep.Role_on_Project = updated.Role_on_Project;
                ep.UpdatedAt = DateTime.UtcNow;
                ep.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(ep);
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