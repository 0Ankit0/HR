using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using HR.Data;

namespace HR.Api
{
    public class RecognitionApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/recognition");

            // Enhanced Awards list: filter, search, paging, exclude deleted
            group.MapGet("/awards", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Awards.Include(a => a.Employee).Where(a => !a.IsDeleted);
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(a => a.Employee_ID == eid);
                // Search by title
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(a => a.Title.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(a => new HR.Models.AwardResponse
                    {
                        Award_ID = a.Award_ID,
                        Title = a.Title,
                        Description = a.Description,
                        DateAwarded = a.DateAwarded,
                        Employee_ID = a.Employee_ID
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            group.MapGet("/awards/{id}", async (int id, AuthDbContext db) =>
                await db.Awards.Include(a => a.Employee).FirstOrDefaultAsync(a => a.Award_ID == id && !a.IsDeleted) is Award award
                    ? Results.Ok(new HR.Models.AwardResponse
                    {
                        Award_ID = award.Award_ID,
                        Title = award.Title,
                        Description = award.Description,
                        DateAwarded = award.DateAwarded,
                        Employee_ID = award.Employee_ID
                    })
                    : Results.NotFound());
            group.MapPost("/awards", async (HR.Models.AwardRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var award = new Award
                {
                    Title = reqModel.Title,
                    Employee_ID = reqModel.Employee_ID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Awards.Add(award);
                await db.SaveChangesAsync();
                return Results.Created($"/api/recognition/awards/{award.Award_ID}", new HR.Models.AwardResponse
                {
                    Award_ID = award.Award_ID,
                    Title = award.Title,
                    Description = award.Description,
                    DateAwarded = award.DateAwarded,
                    Employee_ID = award.Employee_ID
                });
            });
            group.MapPut("/awards/{id}", async (int id, HR.Models.AwardRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var award = await db.Awards.FindAsync(id);
                if (award is null) return Results.NotFound();
                award.Title = reqModel.Title;
                award.Employee_ID = reqModel.Employee_ID;
                award.UpdatedAt = DateTime.UtcNow;
                award.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(new HR.Models.AwardResponse
                {
                    Award_ID = award.Award_ID,
                    Title = award.Title,
                    Description = award.Description,
                    DateAwarded = award.DateAwarded,
                    Employee_ID = award.Employee_ID
                });
            });
            group.MapDelete("/awards/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var award = await db.Awards.FindAsync(id);
                if (award is null) return Results.NotFound();
                award.IsDeleted = true;
                award.UpdatedAt = DateTime.UtcNow;
                award.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Enhanced Nominations list: filter, search, paging, exclude deleted
            group.MapGet("/nominations", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Nominations.Include(n => n.Employee).Where(n => !n.IsDeleted);
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(n => n.Employee_ID == eid);
                // Search by reason
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(n => n.Reason.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(n => new HR.Models.NominationResponse
                    {
                        Nomination_ID = n.Nomination_ID,
                        Employee_ID = n.Employee_ID,
                        Reason = n.Reason,
                        DateNominated = n.DateNominated,
                        IsAwarded = n.IsAwarded
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            group.MapGet("/nominations/{id}", async (int id, AuthDbContext db) =>
                await db.Nominations.Include(n => n.Employee).FirstOrDefaultAsync(n => n.Nomination_ID == id && !n.IsDeleted) is Nomination nomination
                    ? Results.Ok(new HR.Models.NominationResponse
                    {
                        Nomination_ID = nomination.Nomination_ID,
                        Employee_ID = nomination.Employee_ID,
                        Reason = nomination.Reason,
                        DateNominated = nomination.DateNominated,
                        IsAwarded = nomination.IsAwarded
                    })
                    : Results.NotFound());
            group.MapPost("/nominations", async (HR.Models.NominationRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var nomination = new Nomination
                {
                    Employee_ID = reqModel.Employee_ID,
                    Reason = reqModel.Reason,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Nominations.Add(nomination);
                await db.SaveChangesAsync();
                return Results.Created($"/api/recognition/nominations/{nomination.Nomination_ID}", new HR.Models.NominationResponse
                {
                    Nomination_ID = nomination.Nomination_ID,
                    Employee_ID = nomination.Employee_ID,
                    Reason = nomination.Reason,
                    DateNominated = nomination.DateNominated,
                    IsAwarded = nomination.IsAwarded
                });
            });
            group.MapPut("/nominations/{id}", async (int id, HR.Models.NominationRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var nomination = await db.Nominations.FindAsync(id);
                if (nomination is null) return Results.NotFound();
                nomination.Employee_ID = reqModel.Employee_ID;
                nomination.Reason = reqModel.Reason;
                nomination.UpdatedAt = DateTime.UtcNow;
                nomination.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(new HR.Models.NominationResponse
                {
                    Nomination_ID = nomination.Nomination_ID,
                    Employee_ID = nomination.Employee_ID,
                    Reason = nomination.Reason,
                    DateNominated = nomination.DateNominated,
                    IsAwarded = nomination.IsAwarded
                });
            });
            group.MapDelete("/nominations/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var nomination = await db.Nominations.FindAsync(id);
                if (nomination is null) return Results.NotFound();
                nomination.IsDeleted = true;
                nomination.UpdatedAt = DateTime.UtcNow;
                nomination.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
