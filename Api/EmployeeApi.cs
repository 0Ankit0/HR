using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(e => new EmployeeResponse
                    {
                        Employee_ID = e.Employee_ID,
                        Name = e.Name,
                        Email = e.Email,
                        Position = e.Position,
                        HireDate = e.HireDate,
                        Department_ID = e.Department_ID,
                        JobRole_ID = e.JobRole_ID
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/employees/{id}", async (int id, AuthDbContext db) =>
                await db.Employees.FindAsync(id) is Employee e ?
                    Results.Ok(new EmployeeResponse
                    {
                        Employee_ID = e.Employee_ID,
                        Name = e.Name,
                        Email = e.Email,
                        Position = e.Position,
                        HireDate = e.HireDate,
                        Department_ID = e.Department_ID,
                        JobRole_ID = e.JobRole_ID
                    }) : Results.NotFound());

            endpoints.MapPost("/api/employees", async (EmployeeRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var employee = new Employee
                {
                    Name = reqModel.Name,
                    Email = reqModel.Email,
                    Department_ID = reqModel.Department_ID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Employees.Add(employee);
                await db.SaveChangesAsync();
                var response = new EmployeeResponse
                {
                    Employee_ID = employee.Employee_ID,
                    Name = employee.Name,
                    Email = employee.Email,
                    Position = employee.Position,
                    HireDate = employee.HireDate,
                    Department_ID = employee.Department_ID,
                    JobRole_ID = employee.JobRole_ID
                };
                return Results.Created($"/api/employees/{employee.Employee_ID}", response);
            });
            endpoints.MapPut("/api/employees/{id}", async (int id, EmployeeRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var employee = await db.Employees.FindAsync(id);
                if (employee is null) return Results.NotFound();
                employee.Name = reqModel.Name;
                employee.Email = reqModel.Email;
                employee.Department_ID = reqModel.Department_ID;
                employee.UpdatedAt = DateTime.UtcNow;
                employee.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new EmployeeResponse
                {
                    Employee_ID = employee.Employee_ID,
                    Name = employee.Name,
                    Email = employee.Email,
                    Position = employee.Position,
                    HireDate = employee.HireDate,
                    Department_ID = employee.Department_ID,
                    JobRole_ID = employee.JobRole_ID
                };
                return Results.Ok(response);
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
