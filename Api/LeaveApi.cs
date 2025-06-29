using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class LeaveApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/leaves", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Leaves.Where(l => !l.IsDeleted);
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(l => l.Employee_ID == eid);
                // Search by type
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(l => l.LeaveType.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/leaves/{id}", async (int id, AuthDbContext db) =>
                await db.Leaves.FindAsync(id) is Leave l ? Results.Ok(l) : Results.NotFound());

            endpoints.MapPost("/api/leaves", async (Leave leave, AuthDbContext db, HttpContext ctx) =>
            {
                leave.CreatedAt = DateTime.UtcNow;
                leave.CreatedBy = ctx.User?.Identity?.Name;
                db.Leaves.Add(leave);
                await db.SaveChangesAsync();
                return Results.Created($"/api/leaves/{leave.Leave_ID}", leave);
            });
            endpoints.MapPut("/api/leaves/{id}", async (int id, Leave updated, AuthDbContext db, HttpContext ctx) =>
            {
                var leave = await db.Leaves.FindAsync(id);
                if (leave is null) return Results.NotFound();
                leave.StartDate = updated.StartDate;
                leave.EndDate = updated.EndDate;
                leave.LeaveType = updated.LeaveType;
                leave.Status = updated.Status;
                leave.Employee_ID = updated.Employee_ID;
                leave.UpdatedAt = DateTime.UtcNow;
                leave.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(leave);
            });
            endpoints.MapDelete("/api/leaves/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var leave = await db.Leaves.FindAsync(id);
                if (leave is null) return Results.NotFound();
                leave.IsDeleted = true;
                leave.UpdatedAt = DateTime.UtcNow;
                leave.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
