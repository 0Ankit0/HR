using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

namespace HR.Api
{
    public class MentalHealthResourceApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/mentalhealthresources", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.MentalHealthResources.Where(m => !m.IsDeleted);
                // Search by title
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(m => m.Title.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(m => new MentalHealthResourceResponse
                    {
                        MentalHealthResource_ID = m.MentalHealthResource_ID,
                        Title = m.Title,
                        Description = m.Description,
                        ContactInfo = m.ContactInfo
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/mentalhealthresources/{id}", async (int id, AuthDbContext db) =>
                await db.MentalHealthResources.FindAsync(id) is MentalHealthResource m ?
                    Results.Ok(new MentalHealthResourceResponse
                    {
                        MentalHealthResource_ID = m.MentalHealthResource_ID,
                        Title = m.Title,
                        Description = m.Description,
                        ContactInfo = m.ContactInfo
                    }) : Results.NotFound());

            endpoints.MapPost("/api/mentalhealthresources", async (MentalHealthResourceRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var m = new MentalHealthResource
                {
                    Title = reqModel.Title,
                    Description = reqModel.Description,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.MentalHealthResources.Add(m);
                await db.SaveChangesAsync();
                var response = new MentalHealthResourceResponse
                {
                    MentalHealthResource_ID = m.MentalHealthResource_ID,
                    Title = m.Title,
                    Description = m.Description,
                    ContactInfo = m.ContactInfo
                };
                return Results.Created($"/api/mentalhealthresources/{m.MentalHealthResource_ID}", response);
            });
            endpoints.MapPut("/api/mentalhealthresources/{id}", async (int id, MentalHealthResourceRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var m = await db.MentalHealthResources.FindAsync(id);
                if (m is null) return Results.NotFound();
                m.Title = reqModel.Title;
                m.Description = reqModel.Description;
                m.UpdatedAt = DateTime.UtcNow;
                m.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new MentalHealthResourceResponse
                {
                    MentalHealthResource_ID = m.MentalHealthResource_ID,
                    Title = m.Title,
                    Description = m.Description,
                    ContactInfo = m.ContactInfo
                };
                return Results.Ok(response);
            });
            endpoints.MapDelete("/api/mentalhealthresources/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var m = await db.MentalHealthResources.FindAsync(id);
                if (m is null) return Results.NotFound();
                m.IsDeleted = true;
                m.UpdatedAt = DateTime.UtcNow;
                m.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
