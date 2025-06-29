using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/grievances/{id}", async (int id, AuthDbContext db) =>
                await db.Grievances.FindAsync(id) is Grievance g ? Results.Ok(g) : Results.NotFound());

            endpoints.MapPost("/api/grievances", async (Grievance g, AuthDbContext db, HttpContext ctx) =>
            {
                g.CreatedAt = DateTime.UtcNow;
                g.CreatedBy = ctx.User?.Identity?.Name;
                db.Grievances.Add(g);
                await db.SaveChangesAsync();
                return Results.Created($"/api/grievances/{g.Grievance_ID}", g);
            });
            endpoints.MapPut("/api/grievances/{id}", async (int id, Grievance updated, AuthDbContext db, HttpContext ctx) =>
            {
                var g = await db.Grievances.FindAsync(id);
                if (g is null) return Results.NotFound();
                g.Description = updated.Description;
                g.Status = updated.Status;
                g.Employee_ID = updated.Employee_ID;
                g.SubmittedDate = updated.SubmittedDate;
                g.UpdatedAt = DateTime.UtcNow;
                g.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(g);
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