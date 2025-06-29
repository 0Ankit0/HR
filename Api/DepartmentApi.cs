using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(d => new DepartmentResponse
                    {
                        Department_ID = d.Department_ID,
                        Department_Name = d.Department_Name,
                        Department_Location = d.Department_Location,
                        ManagerID = d.ManagerID
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/departments/{id}", async (int id, AuthDbContext db) =>
                await db.Departments.FindAsync(id) is Department d ?
                    Results.Ok(new DepartmentResponse
                    {
                        Department_ID = d.Department_ID,
                        Department_Name = d.Department_Name,
                        Department_Location = d.Department_Location,
                        ManagerID = d.ManagerID
                    }) : Results.NotFound());

            endpoints.MapPost("/api/departments", async (DepartmentRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var department = new Department
                {
                    Department_Name = reqModel.Department_Name,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Departments.Add(department);
                await db.SaveChangesAsync();
                var response = new DepartmentResponse
                {
                    Department_ID = department.Department_ID,
                    Department_Name = department.Department_Name,
                    Department_Location = department.Department_Location,
                    ManagerID = department.ManagerID
                };
                return Results.Created($"/api/departments/{department.Department_ID}", response);
            });
            endpoints.MapPut("/api/departments/{id}", async (int id, DepartmentRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var department = await db.Departments.FindAsync(id);
                if (department is null) return Results.NotFound();
                department.Department_Name = reqModel.Department_Name;
                department.UpdatedAt = DateTime.UtcNow;
                department.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new DepartmentResponse
                {
                    Department_ID = department.Department_ID,
                    Department_Name = department.Department_Name,
                    Department_Location = department.Department_Location,
                    ManagerID = department.ManagerID
                };
                return Results.Ok(response);
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
