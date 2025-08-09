using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using HR.Data;
using HR.Models;

namespace HR.Api
{
    public class NominationApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced Nominations list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/nominations", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Nominations.Include(n => n.Employee).Where(n => !n.IsDeleted);

                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(n => n.Employee_ID == eid);

                // Filter by awarded status
                if (req.Query.TryGetValue("isAwarded", out var awarded) && bool.TryParse(awarded, out var isAwarded))
                    query = query.Where(n => n.IsAwarded == isAwarded);

                // Search by reason
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(n => n.Reason.Contains(q!));

                // Date filtering
                if (req.Query.TryGetValue("startDate", out var startDate) && DateTime.TryParse(startDate, out var start))
                    query = query.Where(n => n.DateNominated >= start);

                if (req.Query.TryGetValue("endDate", out var endDate) && DateTime.TryParse(endDate, out var end))
                    query = query.Where(n => n.DateNominated <= end);

                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;

                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(n => new NominationResponse
                    {
                        Nomination_ID = n.Nomination_ID,
                        Employee_ID = n.Employee_ID,
                        EmployeeName = n.Employee != null ? n.Employee.Name : null,
                        Reason = n.Reason,
                        DateNominated = n.DateNominated,
                        IsAwarded = n.IsAwarded,
                        NominatedBy = n.NominatedBy
                    }).ToListAsync();

                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/nominations/{id}", async (int id, AuthDbContext db) =>
                await db.Nominations.Include(n => n.Employee).FirstOrDefaultAsync(n => n.Nomination_ID == id && !n.IsDeleted) is Nomination n ?
                    Results.Ok(new NominationResponse
                    {
                        Nomination_ID = n.Nomination_ID,
                        Employee_ID = n.Employee_ID,
                        EmployeeName = n.Employee?.Name,
                        Reason = n.Reason,
                        DateNominated = n.DateNominated,
                        IsAwarded = n.IsAwarded,
                        NominatedBy = n.NominatedBy
                    }) : Results.NotFound());

            endpoints.MapPost("/api/nominations", async (NominationRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                // Validate employee exists
                var employeeExists = await db.Employees.AnyAsync(e => e.Employee_ID == reqModel.Employee_ID && !e.IsDeleted);
                if (!employeeExists)
                    return Results.BadRequest("Employee not found");

                var nomination = new Nomination
                {
                    Employee_ID = reqModel.Employee_ID,
                    Reason = reqModel.Reason,
                    DateNominated = reqModel.DateNominated ?? DateTime.UtcNow,
                    IsAwarded = reqModel.IsAwarded,
                    NominatedBy = reqModel.NominatedBy ?? ctx.User?.Identity?.Name,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };

                db.Nominations.Add(nomination);
                await db.SaveChangesAsync();

                // Load employee data for response
                await db.Entry(nomination).Reference(n => n.Employee).LoadAsync();

                return Results.Created($"/api/nominations/{nomination.Nomination_ID}", new NominationResponse
                {
                    Nomination_ID = nomination.Nomination_ID,
                    Employee_ID = nomination.Employee_ID,
                    EmployeeName = nomination.Employee?.Name,
                    Reason = nomination.Reason,
                    DateNominated = nomination.DateNominated,
                    IsAwarded = nomination.IsAwarded,
                    NominatedBy = nomination.NominatedBy
                });
            });

            endpoints.MapPut("/api/nominations/{id}", async (int id, NominationRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var nomination = await db.Nominations.Include(n => n.Employee).FirstOrDefaultAsync(n => n.Nomination_ID == id && !n.IsDeleted);
                if (nomination is null) return Results.NotFound();

                // Validate employee exists if changing employee
                if (nomination.Employee_ID != reqModel.Employee_ID)
                {
                    var employeeExists = await db.Employees.AnyAsync(e => e.Employee_ID == reqModel.Employee_ID && !e.IsDeleted);
                    if (!employeeExists)
                        return Results.BadRequest("Employee not found");
                }

                nomination.Employee_ID = reqModel.Employee_ID;
                nomination.Reason = reqModel.Reason;
                nomination.DateNominated = reqModel.DateNominated ?? nomination.DateNominated;
                nomination.IsAwarded = reqModel.IsAwarded;
                if (!string.IsNullOrEmpty(reqModel.NominatedBy))
                    nomination.NominatedBy = reqModel.NominatedBy;
                nomination.UpdatedAt = DateTime.UtcNow;
                nomination.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();

                return Results.Ok(new NominationResponse
                {
                    Nomination_ID = nomination.Nomination_ID,
                    Employee_ID = nomination.Employee_ID,
                    EmployeeName = nomination.Employee?.Name,
                    Reason = nomination.Reason,
                    DateNominated = nomination.DateNominated,
                    IsAwarded = nomination.IsAwarded,
                    NominatedBy = nomination.NominatedBy
                });
            });

            // Award a nomination
            endpoints.MapPatch("/api/nominations/{id}/award", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var nomination = await db.Nominations.Include(n => n.Employee).FirstOrDefaultAsync(n => n.Nomination_ID == id && !n.IsDeleted);
                if (nomination is null) return Results.NotFound();

                nomination.IsAwarded = true;
                nomination.UpdatedAt = DateTime.UtcNow;
                nomination.UpdatedBy = ctx.User?.Identity?.Name;

                // Optionally create an Award record
                var award = new Award
                {
                    Title = $"Nominated Award - {nomination.Reason}",
                    Description = $"Award based on nomination: {nomination.Reason}",
                    Employee_ID = nomination.Employee_ID,
                    DateAwarded = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };

                db.Awards.Add(award);
                await db.SaveChangesAsync();

                return Results.Ok(new { Message = "Nomination awarded successfully", AwardId = award.Award_ID });
            });

            endpoints.MapDelete("/api/nominations/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var nomination = await db.Nominations.FindAsync(id);
                if (nomination is null || nomination.IsDeleted) return Results.NotFound();

                // Soft delete
                nomination.IsDeleted = true;
                nomination.UpdatedAt = DateTime.UtcNow;
                nomination.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Get pending nominations (not yet awarded)
            endpoints.MapGet("/api/nominations/pending", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Nominations.Include(n => n.Employee)
                    .Where(n => !n.IsDeleted && !n.IsAwarded);

                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;

                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(n => new NominationResponse
                    {
                        Nomination_ID = n.Nomination_ID,
                        Employee_ID = n.Employee_ID,
                        EmployeeName = n.Employee != null ? n.Employee.Name : null,
                        Reason = n.Reason,
                        DateNominated = n.DateNominated,
                        IsAwarded = n.IsAwarded,
                        NominatedBy = n.NominatedBy
                    }).ToListAsync();

                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
        }
    }
}
