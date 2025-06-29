using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;

namespace HR.Api
{
    public class IncidentApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/incidents");

            // Only keep enhanced Incident endpoints
            group.MapGet("/list", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Incidents.Include(i => i.Employee).Where(i => !i.IsDeleted);
                // Filtering by status
                if (req.Query.TryGetValue("status", out var status))
                    query = query.Where(i => i.Status == status);
                // Search by description
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(i => i.Description.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            group.MapGet("/list/{id}", async (int id, AuthDbContext db) =>
                await db.Incidents.Include(i => i.Employee).FirstOrDefaultAsync(i => i.Incident_ID == id && !i.IsDeleted) is Incident incident
                    ? Results.Ok(incident)
                    : Results.NotFound());
            group.MapPost("/report", async (Incident incident, AuthDbContext db, HttpContext ctx) =>
            {
                incident.CreatedAt = DateTime.UtcNow;
                incident.CreatedBy = ctx.User?.Identity?.Name;
                db.Incidents.Add(incident);
                await db.SaveChangesAsync();
                return Results.Created($"/api/incidents/list/{incident.Incident_ID}", incident);
            });
            group.MapPut("/list/{id}", async (int id, Incident updated, AuthDbContext db, HttpContext ctx) =>
            {
                var incident = await db.Incidents.FindAsync(id);
                if (incident is null) return Results.NotFound();
                incident.Description = updated.Description;
                incident.DateReported = updated.DateReported;
                incident.Status = updated.Status;
                incident.Employee_ID = updated.Employee_ID;
                incident.UpdatedAt = DateTime.UtcNow;
                incident.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(incident);
            });
            group.MapDelete("/list/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var incident = await db.Incidents.FindAsync(id);
                if (incident is null) return Results.NotFound();
                incident.IsDeleted = true;
                incident.UpdatedAt = DateTime.UtcNow;
                incident.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
