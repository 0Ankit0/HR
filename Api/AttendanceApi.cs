using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(a => new AttendanceResponse
                    {
                        Attendance_ID = a.Attendance_ID,
                        Employee_ID = a.Employee_ID,
                        Date = a.Date,
                        Status = a.Status,
                        TimeIn = a.TimeIn,
                        TimeOut = a.TimeOut
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/attendances/{id}", async (int id, AuthDbContext db) =>
                await db.Attendances.FindAsync(id) is Attendance a ?
                    Results.Ok(new AttendanceResponse
                    {
                        Attendance_ID = a.Attendance_ID,
                        Employee_ID = a.Employee_ID,
                        Date = a.Date,
                        Status = a.Status,
                        TimeIn = a.TimeIn,
                        TimeOut = a.TimeOut
                    }) : Results.NotFound());

            endpoints.MapPost("/api/attendances", async (AttendanceRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var attendance = new Attendance
                {
                    Employee_ID = reqModel.Employee_ID,
                    Date = reqModel.Date,
                    Status = reqModel.Status,
                    TimeIn = reqModel.TimeIn,
                    TimeOut = reqModel.TimeOut,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Attendances.Add(attendance);
                await db.SaveChangesAsync();
                var response = new AttendanceResponse
                {
                    Attendance_ID = attendance.Attendance_ID,
                    Employee_ID = attendance.Employee_ID,
                    Date = attendance.Date,
                    Status = attendance.Status,
                    TimeIn = attendance.TimeIn,
                    TimeOut = attendance.TimeOut
                };
                return Results.Created($"/api/attendances/{attendance.Attendance_ID}", response);
            });

            endpoints.MapPut("/api/attendances/{id}", async (int id, AttendanceRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var attendance = await db.Attendances.FindAsync(id);
                if (attendance is null) return Results.NotFound();
                attendance.Employee_ID = reqModel.Employee_ID;
                attendance.Date = reqModel.Date;
                attendance.Status = reqModel.Status;
                attendance.TimeIn = reqModel.TimeIn;
                attendance.TimeOut = reqModel.TimeOut;
                attendance.UpdatedAt = DateTime.UtcNow;
                attendance.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new AttendanceResponse
                {
                    Attendance_ID = attendance.Attendance_ID,
                    Employee_ID = attendance.Employee_ID,
                    Date = attendance.Date,
                    Status = attendance.Status,
                    TimeIn = attendance.TimeIn,
                    TimeOut = attendance.TimeOut
                };
                return Results.Ok(response);
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
