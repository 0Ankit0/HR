using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using HR.Data;
using HR.Models;

namespace HR.Api
{
    public class AwardApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced Awards list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/awards", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Awards.Include(a => a.Employee).Where(a => !a.IsDeleted);
                
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(a => a.Employee_ID == eid);
                
                // Search by title or description
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(a => a.Title.Contains(q!) || (a.Description != null && a.Description.Contains(q!)));
                
                // Date filtering
                if (req.Query.TryGetValue("startDate", out var startDate) && DateTime.TryParse(startDate, out var start))
                    query = query.Where(a => a.DateAwarded >= start);
                
                if (req.Query.TryGetValue("endDate", out var endDate) && DateTime.TryParse(endDate, out var end))
                    query = query.Where(a => a.DateAwarded <= end);
                
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(a => new AwardResponse
                    {
                        Award_ID = a.Award_ID,
                        Title = a.Title,
                        Description = a.Description,
                        DateAwarded = a.DateAwarded,
                        Employee_ID = a.Employee_ID,
                        EmployeeName = a.Employee != null ? a.Employee.Name : null
                    }).ToListAsync();
                
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/awards/{id}", async (int id, AuthDbContext db) =>
                await db.Awards.Include(a => a.Employee).FirstOrDefaultAsync(a => a.Award_ID == id && !a.IsDeleted) is Award a ?
                    Results.Ok(new AwardResponse
                    {
                        Award_ID = a.Award_ID,
                        Title = a.Title,
                        Description = a.Description,
                        DateAwarded = a.DateAwarded,
                        Employee_ID = a.Employee_ID,
                        EmployeeName = a.Employee?.Name
                    }) : Results.NotFound());

            endpoints.MapPost("/api/awards", async (AwardRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                // Validate employee exists
                var employeeExists = await db.Employees.AnyAsync(e => e.Employee_ID == reqModel.Employee_ID && !e.IsDeleted);
                if (!employeeExists)
                    return Results.BadRequest("Employee not found");

                var award = new Award
                {
                    Title = reqModel.Title,
                    Description = reqModel.Description,
                    Employee_ID = reqModel.Employee_ID,
                    DateAwarded = reqModel.DateAwarded ?? DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                
                db.Awards.Add(award);
                await db.SaveChangesAsync();
                
                // Load employee data for response
                await db.Entry(award).Reference(a => a.Employee).LoadAsync();
                
                return Results.Created($"/api/awards/{award.Award_ID}", new AwardResponse
                {
                    Award_ID = award.Award_ID,
                    Title = award.Title,
                    Description = award.Description,
                    DateAwarded = award.DateAwarded,
                    Employee_ID = award.Employee_ID,
                    EmployeeName = award.Employee?.Name
                });
            });

            endpoints.MapPut("/api/awards/{id}", async (int id, AwardRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var award = await db.Awards.Include(a => a.Employee).FirstOrDefaultAsync(a => a.Award_ID == id && !a.IsDeleted);
                if (award is null) return Results.NotFound();

                // Validate employee exists if changing employee
                if (award.Employee_ID != reqModel.Employee_ID)
                {
                    var employeeExists = await db.Employees.AnyAsync(e => e.Employee_ID == reqModel.Employee_ID && !e.IsDeleted);
                    if (!employeeExists)
                        return Results.BadRequest("Employee not found");
                }

                award.Title = reqModel.Title;
                award.Description = reqModel.Description;
                award.Employee_ID = reqModel.Employee_ID;
                award.DateAwarded = reqModel.DateAwarded ?? award.DateAwarded;
                award.UpdatedAt = DateTime.UtcNow;
                award.UpdatedBy = ctx.User?.Identity?.Name;
                
                await db.SaveChangesAsync();
                
                return Results.Ok(new AwardResponse
                {
                    Award_ID = award.Award_ID,
                    Title = award.Title,
                    Description = award.Description,
                    DateAwarded = award.DateAwarded,
                    Employee_ID = award.Employee_ID,
                    EmployeeName = award.Employee?.Name
                });
            });

            endpoints.MapDelete("/api/awards/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var award = await db.Awards.FindAsync(id);
                if (award is null || award.IsDeleted) return Results.NotFound();
                
                // Soft delete
                award.IsDeleted = true;
                award.UpdatedAt = DateTime.UtcNow;
                award.UpdatedBy = ctx.User?.Identity?.Name;
                
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Bulk operations
            endpoints.MapPost("/api/awards/bulk", async (List<AwardRequest> requests, AuthDbContext db, HttpContext ctx) =>
            {
                var awards = new List<Award>();
                foreach (var req in requests)
                {
                    var employeeExists = await db.Employees.AnyAsync(e => e.Employee_ID == req.Employee_ID && !e.IsDeleted);
                    if (!employeeExists) continue;

                    awards.Add(new Award
                    {
                        Title = req.Title,
                        Description = req.Description,
                        Employee_ID = req.Employee_ID,
                        DateAwarded = req.DateAwarded ?? DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = ctx.User?.Identity?.Name
                    });
                }

                db.Awards.AddRange(awards);
                await db.SaveChangesAsync();
                return Results.Ok(new { Created = awards.Count });
            });
        }
    }
}
