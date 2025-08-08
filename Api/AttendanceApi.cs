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
                var query = from a in db.Attendances.Where(a => !a.IsDeleted)
                            join e in db.Employees on a.Employee_ID equals e.Employee_ID into employeeGroup
                            from emp in employeeGroup.DefaultIfEmpty()
                            join d in db.Departments on emp.Department_ID equals d.Department_ID into deptGroup
                            from dept in deptGroup.DefaultIfEmpty()
                            select new { Attendance = a, Employee = emp, Department = dept };

                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(x => x.Attendance.Employee_ID == eid);

                // Filtering by department
                if (req.Query.TryGetValue("department", out var deptName) && !string.IsNullOrEmpty(deptName))
                    query = query.Where(x => x.Department != null && x.Department.Department_Name == deptName);

                // Filtering by status
                if (req.Query.TryGetValue("status", out var status) && !string.IsNullOrEmpty(status))
                    query = query.Where(x => x.Attendance.Status == status);

                // Date range filtering
                if (req.Query.TryGetValue("date", out var dateStr) && DateTime.TryParse(dateStr, out var date))
                    query = query.Where(x => x.Attendance.Date.Date == date.Date);

                if (req.Query.TryGetValue("fromDate", out var fromDateStr) && DateTime.TryParse(fromDateStr, out var fromDate))
                    query = query.Where(x => x.Attendance.Date >= fromDate);

                if (req.Query.TryGetValue("toDate", out var toDateStr) && DateTime.TryParse(toDateStr, out var toDate))
                    query = query.Where(x => x.Attendance.Date <= toDate);

                // Search by employee name
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(x => x.Employee != null && x.Employee.Name.Contains(q!));

                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(x => new AttendanceResponseWithEmployee
                    {
                        Attendance_ID = x.Attendance.Attendance_ID,
                        Employee_ID = x.Attendance.Employee_ID,
                        EmployeeName = x.Employee != null ? x.Employee.Name : "Unknown",
                        Department = x.Department != null ? x.Department.Department_Name : "Unknown",
                        Date = x.Attendance.Date,
                        Status = x.Attendance.Status,
                        TimeIn = x.Attendance.TimeIn,
                        TimeOut = x.Attendance.TimeOut,
                        HoursWorked = x.Attendance.TimeOut != TimeSpan.Zero && x.Attendance.TimeIn != TimeSpan.Zero ?
                            (x.Attendance.TimeOut - x.Attendance.TimeIn).TotalHours : null
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
                    TimeIn = reqModel.TimeIn ?? TimeSpan.Zero,
                    TimeOut = reqModel.TimeOut ?? TimeSpan.Zero,
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
                attendance.TimeIn = reqModel.TimeIn ?? TimeSpan.Zero;
                attendance.TimeOut = reqModel.TimeOut ?? TimeSpan.Zero;
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

            // Get attendance statistics
            endpoints.MapGet("/api/attendances/stats", async (HttpRequest req, AuthDbContext db) =>
            {
                var today = DateTime.Today;
                var query = db.Attendances.Where(a => !a.IsDeleted);

                // Filter by date if provided
                if (req.Query.TryGetValue("date", out var dateStr) && DateTime.TryParse(dateStr, out var date))
                {
                    query = query.Where(a => a.Date.Date == date.Date);
                }
                else
                {
                    query = query.Where(a => a.Date.Date == today);
                }

                var total = await query.CountAsync();
                var present = await query.CountAsync(a => a.Status == "Present");
                var absent = await query.CountAsync(a => a.Status == "Absent");
                var late = await query.CountAsync(a => a.Status == "Late");
                var halfDay = await query.CountAsync(a => a.Status == "Half Day");

                var attendanceRate = total > 0 ? (double)present / total * 100 : 0;

                return Results.Ok(new
                {
                    Total = total,
                    Present = present,
                    Absent = absent,
                    Late = late,
                    HalfDay = halfDay,
                    AttendanceRate = Math.Round(attendanceRate, 1)
                });
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
