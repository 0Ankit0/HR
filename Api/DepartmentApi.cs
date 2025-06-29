using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class DepartmentApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/departments", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Departments.Where(d => !d.IsDeleted);
                // Search by name
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(d => d.Department_Name.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/departments/{id}", async (int id, AuthDbContext db) =>
                await db.Departments.FindAsync(id) is Department d ? Results.Ok(d) : Results.NotFound());

            endpoints.MapPost("/api/departments", async (Department department, AuthDbContext db, HttpContext ctx) =>
            {
                department.CreatedAt = DateTime.UtcNow;
                department.CreatedBy = ctx.User?.Identity?.Name;
                db.Departments.Add(department);
                await db.SaveChangesAsync();
                return Results.Created($"/api/departments/{department.Department_ID}", department);
            });
            endpoints.MapPut("/api/departments/{id}", async (int id, Department updated, AuthDbContext db, HttpContext ctx) =>
            {
                var department = await db.Departments.FindAsync(id);
                if (department is null) return Results.NotFound();
                department.Department_Name = updated.Department_Name;
                department.Department_Location = updated.Department_Location;
                department.ManagerID = updated.ManagerID;
                department.UpdatedAt = DateTime.UtcNow;
                department.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(department);
            });
            endpoints.MapDelete("/api/departments/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var department = await db.Departments.FindAsync(id);
                if (department is null) return Results.NotFound();
                department.IsDeleted = true;
                department.UpdatedAt = DateTime.UtcNow;
                department.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
