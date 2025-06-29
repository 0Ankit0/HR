using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class AttendanceApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/attendances", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Attendances.Where(a => !a.IsDeleted);
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(a => a.Employee_ID == eid);
                // Search by status
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(a => a.Status.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/attendances/{id}", async (int id, AuthDbContext db) =>
                await db.Attendances.FindAsync(id) is Attendance a ? Results.Ok(a) : Results.NotFound());

            endpoints.MapPost("/api/attendances", async (Attendance attendance, AuthDbContext db, HttpContext ctx) =>
            {
                attendance.CreatedAt = DateTime.UtcNow;
                attendance.CreatedBy = ctx.User?.Identity?.Name;
                db.Attendances.Add(attendance);
                await db.SaveChangesAsync();
                return Results.Created($"/api/attendances/{attendance.Attendance_ID}", attendance);
            });
            endpoints.MapPut("/api/attendances/{id}", async (int id, Attendance updated, AuthDbContext db, HttpContext ctx) =>
            {
                var attendance = await db.Attendances.FindAsync(id);
                if (attendance is null) return Results.NotFound();
                attendance.Date = updated.Date;
                attendance.Status = updated.Status;
                attendance.TimeIn = updated.TimeIn;
                attendance.TimeOut = updated.TimeOut;
                attendance.Employee_ID = updated.Employee_ID;
                attendance.UpdatedAt = DateTime.UtcNow;
                attendance.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(attendance);
            });
            endpoints.MapDelete("/api/attendances/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var attendance = await db.Attendances.FindAsync(id);
                if (attendance is null) return Results.NotFound();
                attendance.IsDeleted = true;
                attendance.UpdatedAt = DateTime.UtcNow;
                attendance.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
