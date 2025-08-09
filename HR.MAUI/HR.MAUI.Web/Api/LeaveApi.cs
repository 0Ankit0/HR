using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(l => new LeaveResponse
                    {
                        Leave_ID = l.Leave_ID,
                        Employee_ID = l.Employee_ID,
                        StartDate = l.StartDate,
                        EndDate = l.EndDate,
                        LeaveType = l.LeaveType,
                        Status = l.Status
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/leaves/{id}", async (int id, AuthDbContext db) =>
                await db.Leaves.FindAsync(id) is Leave l ?
                    Results.Ok(new LeaveResponse
                    {
                        Leave_ID = l.Leave_ID,
                        Employee_ID = l.Employee_ID,
                        StartDate = l.StartDate,
                        EndDate = l.EndDate,
                        LeaveType = l.LeaveType,
                        Status = l.Status
                    }) : Results.NotFound());

            endpoints.MapPost("/api/leaves", async (LeaveRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var leave = new Leave
                {
                    Employee_ID = reqModel.Employee_ID,
                    StartDate = reqModel.StartDate,
                    EndDate = reqModel.EndDate,
                    LeaveType = reqModel.LeaveType,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Leaves.Add(leave);
                await db.SaveChangesAsync();
                var response = new LeaveResponse
                {
                    Leave_ID = leave.Leave_ID,
                    Employee_ID = leave.Employee_ID,
                    StartDate = leave.StartDate,
                    EndDate = leave.EndDate,
                    LeaveType = leave.LeaveType,
                    Status = leave.Status
                };
                return Results.Created($"/api/leaves/{leave.Leave_ID}", response);
            });
            endpoints.MapPut("/api/leaves/{id}", async (int id, LeaveRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var leave = await db.Leaves.FindAsync(id);
                if (leave is null) return Results.NotFound();
                leave.Employee_ID = reqModel.Employee_ID;
                leave.StartDate = reqModel.StartDate;
                leave.EndDate = reqModel.EndDate;
                leave.LeaveType = reqModel.LeaveType;
                leave.UpdatedAt = DateTime.UtcNow;
                leave.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new LeaveResponse
                {
                    Leave_ID = leave.Leave_ID,
                    Employee_ID = leave.Employee_ID,
                    StartDate = leave.StartDate,
                    EndDate = leave.EndDate,
                    LeaveType = leave.LeaveType,
                    Status = leave.Status
                };
                return Results.Ok(response);
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
