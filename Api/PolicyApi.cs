using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

namespace HR.Api
{
    public class PolicyApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/policies", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Policies.Where(p => !p.IsDeleted);
                // Search by title
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(p => p.Title.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(p => new PolicyResponse
                    {
                        Policy_ID = p.Policy_ID,
                        Title = p.Title,
                        Content = p.Content,
                        EffectiveDate = p.EffectiveDate
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            endpoints.MapGet("/api/policies/{id}", async (int id, AuthDbContext db) =>
                await db.Policies.FindAsync(id) is Policy p ?
                    Results.Ok(new PolicyResponse
                    {
                        Policy_ID = p.Policy_ID,
                        Title = p.Title,
                        Content = p.Content,
                        EffectiveDate = p.EffectiveDate
                    }) : Results.NotFound());
            endpoints.MapPost("/api/policies", async (PolicyRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var policy = new Policy
                {
                    Title = reqModel.Title,
                    Content = reqModel.Content,
                    EffectiveDate = reqModel.EffectiveDate
                };
                db.Policies.Add(policy);
                await db.SaveChangesAsync();
                return Results.Created($"/api/policies/{policy.Policy_ID}", new PolicyResponse
                {
                    Policy_ID = policy.Policy_ID,
                    Title = policy.Title,
                    Content = policy.Content,
                    EffectiveDate = policy.EffectiveDate
                });
            });
            endpoints.MapPut("/api/policies/{id}", async (int id, PolicyRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var p = await db.Policies.FindAsync(id);
                if (p is null) return Results.NotFound();
                p.Title = reqModel.Title;
                p.Content = reqModel.Content;
                p.EffectiveDate = reqModel.EffectiveDate;
                await db.SaveChangesAsync();
                return Results.Ok(new PolicyResponse
                {
                    Policy_ID = p.Policy_ID,
                    Title = p.Title,
                    Content = p.Content,
                    EffectiveDate = p.EffectiveDate
                });
            });
            endpoints.MapDelete("/api/policies/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var p = await db.Policies.FindAsync(id);
                if (p is null) return Results.NotFound();
                p.IsDeleted = true;
                p.UpdatedAt = DateTime.UtcNow;
                p.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}