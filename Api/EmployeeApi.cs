using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
// Use explicit alias for Data.Employee to avoid ambiguity
using DataEmployee = HR.Data.Employee;
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
                var query = db.Employees.Include(e => e.Department).Include(e => e.JobRole).Where(e => !e.IsDeleted);

                // Filtering by department
                if (req.Query.TryGetValue("departmentId", out var deptId) && int.TryParse(deptId, out var did))
                    query = query.Where(e => e.Department_ID == did);

                // Search by name or email
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(e => e.Name.Contains(q!) || e.Email.Contains(q!));

                // Filter by position
                if (req.Query.TryGetValue("position", out var position) && !string.IsNullOrEmpty(position))
                    query = query.Where(e => e.Position.Contains(position!));

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
                        Address = e.Address,
                        PhoneNumber = e.PhoneNumber,
                        Position = e.Position,
                        HireDate = e.HireDate,
                        Department_ID = e.Department_ID,
                        DepartmentName = e.Department != null ? e.Department.Department_Name : "",
                        JobRole_ID = e.JobRole_ID,
                        JobRoleName = e.JobRole != null ? e.JobRole.Role_Name : ""
                    }).ToListAsync();

                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/employees/{id}", async (int id, AuthDbContext db) =>
            {
                var employee = await db.Employees
                    .Include(e => e.Department)
                    .Include(e => e.JobRole)
                    .FirstOrDefaultAsync(e => e.Employee_ID == id && !e.IsDeleted);

                if (employee == null) return Results.NotFound();

                return Results.Ok(new EmployeeResponse
                {
                    Employee_ID = employee.Employee_ID,
                    Name = employee.Name,
                    Email = employee.Email,
                    Address = employee.Address,
                    PhoneNumber = employee.PhoneNumber,
                    Position = employee.Position,
                    HireDate = employee.HireDate,
                    Department_ID = employee.Department_ID,
                    DepartmentName = employee.Department?.Department_Name ?? "",
                    JobRole_ID = employee.JobRole_ID,
                    JobRoleName = employee.JobRole?.Role_Name ?? ""
                });
            });

            endpoints.MapPost("/api/employees", async (EmployeeRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var employee = new DataEmployee
                {
                    Name = reqModel.Name,
                    Email = reqModel.Email,
                    Address = reqModel.Address,
                    PhoneNumber = reqModel.PhoneNumber,
                    Position = reqModel.Position,
                    HireDate = reqModel.HireDate,
                    Department_ID = reqModel.Department_ID,
                    JobRole_ID = reqModel.JobRole_ID,
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
                    Address = employee.Address,
                    PhoneNumber = employee.PhoneNumber,
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
                if (employee == null || employee.IsDeleted) return Results.NotFound();

                employee.Name = reqModel.Name;
                employee.Email = reqModel.Email;
                employee.Address = reqModel.Address;
                employee.PhoneNumber = reqModel.PhoneNumber;
                employee.Position = reqModel.Position;
                employee.HireDate = reqModel.HireDate;
                employee.Department_ID = reqModel.Department_ID;
                employee.JobRole_ID = reqModel.JobRole_ID;
                employee.UpdatedAt = DateTime.UtcNow;
                employee.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();

                var response = new EmployeeResponse
                {
                    Employee_ID = employee.Employee_ID,
                    Name = employee.Name,
                    Email = employee.Email,
                    Address = employee.Address,
                    PhoneNumber = employee.PhoneNumber,
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
                if (employee == null || employee.IsDeleted) return Results.NotFound();

                employee.IsDeleted = true;
                employee.UpdatedAt = DateTime.UtcNow;
                employee.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Stats endpoint
            endpoints.MapGet("/api/employees/stats", async (AuthDbContext db) =>
            {
                var employees = await db.Employees.Where(e => !e.IsDeleted).ToListAsync();
                var totalEmployees = employees.Count;
                var newHiresThisMonth = employees.Count(e => e.HireDate >= DateTime.Now.AddDays(-30));
                var departmentStats = await db.Employees
                    .Where(e => !e.IsDeleted)
                    .Include(e => e.Department)
                    .GroupBy(e => e.Department!.Department_Name)
                    .Select(g => new { Department = g.Key, Count = g.Count() })
                    .ToListAsync();

                return Results.Ok(new
                {
                    TotalEmployees = totalEmployees,
                    NewHiresThisMonth = newHiresThisMonth,
                    ActiveEmployees = totalEmployees, // Assuming all non-deleted are active
                    DepartmentDistribution = departmentStats
                });
            });
        }
    }
}
