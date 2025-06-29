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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            group.MapGet("/awards/{id}", async (int id, AuthDbContext db) =>
                await db.Awards.Include(a => a.Employee).FirstOrDefaultAsync(a => a.Award_ID == id && !a.IsDeleted) is Award award
                    ? Results.Ok(award)
                    : Results.NotFound());
            group.MapPost("/awards", async (Award award, AuthDbContext db, HttpContext ctx) =>
            {
                award.CreatedAt = DateTime.UtcNow;
                award.CreatedBy = ctx.User?.Identity?.Name;
                db.Awards.Add(award);
                await db.SaveChangesAsync();
                return Results.Created($"/api/recognition/awards/{award.Award_ID}", award);
            });
            group.MapPut("/awards/{id}", async (int id, Award updated, AuthDbContext db, HttpContext ctx) =>
            {
                var award = await db.Awards.FindAsync(id);
                if (award is null) return Results.NotFound();
                award.Title = updated.Title;
                award.Description = updated.Description;
                award.DateAwarded = updated.DateAwarded;
                award.Employee_ID = updated.Employee_ID;
                award.UpdatedAt = DateTime.UtcNow;
                award.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(award);
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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            group.MapGet("/nominations/{id}", async (int id, AuthDbContext db) =>
                await db.Nominations.Include(n => n.Employee).FirstOrDefaultAsync(n => n.Nomination_ID == id && !n.IsDeleted) is Nomination nomination
                    ? Results.Ok(nomination)
                    : Results.NotFound());
            group.MapPost("/nominations", async (Nomination nomination, AuthDbContext db, HttpContext ctx) =>
            {
                nomination.CreatedAt = DateTime.UtcNow;
                nomination.CreatedBy = ctx.User?.Identity?.Name;
                db.Nominations.Add(nomination);
                await db.SaveChangesAsync();
                return Results.Created($"/api/recognition/nominations/{nomination.Nomination_ID}", nomination);
            });
            group.MapPut("/nominations/{id}", async (int id, Nomination updated, AuthDbContext db, HttpContext ctx) =>
            {
                var nomination = await db.Nominations.FindAsync(id);
                if (nomination is null) return Results.NotFound();
                nomination.Reason = updated.Reason;
                nomination.DateNominated = updated.DateNominated;
                nomination.IsAwarded = updated.IsAwarded;
                nomination.Employee_ID = updated.Employee_ID;
                nomination.UpdatedAt = DateTime.UtcNow;
                nomination.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(nomination);
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
