using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

namespace HR.Api
{
    public class DEIResourceApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/deiresources", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.DEIResources.Where(d => !d.IsDeleted);
                // Search by title
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(d => d.Title.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(d => new DEIResourceResponse
                    {
                        DEIResource_ID = d.DEIResource_ID,
                        Title = d.Title,
                        Content = d.Content
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/deiresources/{id}", async (int id, AuthDbContext db) =>
                await db.DEIResources.FindAsync(id) is DEIResource d ?
                    Results.Ok(new DEIResourceResponse
                    {
                        DEIResource_ID = d.DEIResource_ID,
                        Title = d.Title,
                        Content = d.Content
                    }) : Results.NotFound());

            endpoints.MapPost("/api/deiresources", async (DEIResourceRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var d = new DEIResource
                {
                    Title = reqModel.Title,
                    Content = reqModel.Content,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.DEIResources.Add(d);
                await db.SaveChangesAsync();
                var response = new DEIResourceResponse
                {
                    DEIResource_ID = d.DEIResource_ID,
                    Title = d.Title,
                    Content = d.Content
                };
                return Results.Created($"/api/deiresources/{d.DEIResource_ID}", response);
            });
            endpoints.MapPut("/api/deiresources/{id}", async (int id, DEIResourceRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var d = await db.DEIResources.FindAsync(id);
                if (d is null) return Results.NotFound();
                d.Title = reqModel.Title;
                d.Content = reqModel.Content;
                d.UpdatedAt = DateTime.UtcNow;
                d.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new DEIResourceResponse
                {
                    DEIResource_ID = d.DEIResource_ID,
                    Title = d.Title,
                    Content = d.Content
                };
                return Results.Ok(response);
            });
            endpoints.MapDelete("/api/deiresources/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var d = await db.DEIResources.FindAsync(id);
                if (d is null) return Results.NotFound();
                d.IsDeleted = true;
                d.UpdatedAt = DateTime.UtcNow;
                d.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}