using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class EmployeeApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/employees", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Employees.Where(e => !e.IsDeleted);
                // Filtering by department
                if (req.Query.TryGetValue("departmentId", out var deptId) && int.TryParse(deptId, out var did))
                    query = query.Where(e => e.Department_ID == did);
                // Search by name
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(e => e.Name.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/employees/{id}", async (int id, AuthDbContext db) =>
                await db.Employees.FindAsync(id) is Employee e ? Results.Ok(e) : Results.NotFound());

            endpoints.MapPost("/api/employees", async (Employee employee, AuthDbContext db, HttpContext ctx) =>
            {
                employee.CreatedAt = DateTime.UtcNow;
                employee.CreatedBy = ctx.User?.Identity?.Name;
                db.Employees.Add(employee);
                await db.SaveChangesAsync();
                return Results.Created($"/api/employees/{employee.Employee_ID}", employee);
            });
            endpoints.MapPut("/api/employees/{id}", async (int id, Employee updated, AuthDbContext db, HttpContext ctx) =>
            {
                var employee = await db.Employees.FindAsync(id);
                if (employee is null) return Results.NotFound();
                employee.Name = updated.Name;
                employee.Address = updated.Address;
                employee.Email = updated.Email;
                employee.PhoneNumber = updated.PhoneNumber;
                employee.Position = updated.Position;
                employee.HireDate = updated.HireDate;
                employee.Department_ID = updated.Department_ID;
                employee.JobRole_ID = updated.JobRole_ID;
                employee.UpdatedAt = DateTime.UtcNow;
                employee.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(employee);
            });
            endpoints.MapDelete("/api/employees/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var employee = await db.Employees.FindAsync(id);
                if (employee is null) return Results.NotFound();
                employee.IsDeleted = true;
                employee.UpdatedAt = DateTime.UtcNow;
                employee.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
