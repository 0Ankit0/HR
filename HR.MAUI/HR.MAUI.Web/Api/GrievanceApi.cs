using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

namespace HR.Api
{
    public class GrievanceApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/grievances", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Grievances.Where(g => !g.IsDeleted);
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(g => g.Employee_ID == eid);
                // Search by status
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(g => g.Status.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(g => new GrievanceResponse
                    {
                        Grievance_ID = g.Grievance_ID,
                        Employee_ID = g.Employee_ID,
                        Description = g.Description,
                        SubmittedDate = g.SubmittedDate,
                        Status = g.Status
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/grievances/{id}", async (int id, AuthDbContext db) =>
                await db.Grievances.FindAsync(id) is Grievance g ?
                    Results.Ok(new GrievanceResponse
                    {
                        Grievance_ID = g.Grievance_ID,
                        Employee_ID = g.Employee_ID,
                        Description = g.Description,
                        SubmittedDate = g.SubmittedDate,
                        Status = g.Status
                    }) : Results.NotFound());

            endpoints.MapPost("/api/grievances", async (GrievanceRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var g = new Grievance
                {
                    Employee_ID = reqModel.Employee_ID,
                    Description = reqModel.Description,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Grievances.Add(g);
                await db.SaveChangesAsync();
                var response = new GrievanceResponse
                {
                    Grievance_ID = g.Grievance_ID,
                    Employee_ID = g.Employee_ID,
                    Description = g.Description,
                    SubmittedDate = g.SubmittedDate,
                    Status = g.Status
                };
                return Results.Created($"/api/grievances/{g.Grievance_ID}", response);
            });
            endpoints.MapPut("/api/grievances/{id}", async (int id, GrievanceRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var g = await db.Grievances.FindAsync(id);
                if (g is null) return Results.NotFound();
                g.Employee_ID = reqModel.Employee_ID;
                g.Description = reqModel.Description;
                g.UpdatedAt = DateTime.UtcNow;
                g.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new GrievanceResponse
                {
                    Grievance_ID = g.Grievance_ID,
                    Employee_ID = g.Employee_ID,
                    Description = g.Description,
                    SubmittedDate = g.SubmittedDate,
                    Status = g.Status
                };
                return Results.Ok(response);
            });
            endpoints.MapDelete("/api/grievances/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var g = await db.Grievances.FindAsync(id);
                if (g is null) return Results.NotFound();
                g.IsDeleted = true;
                g.UpdatedAt = DateTime.UtcNow;
                g.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}