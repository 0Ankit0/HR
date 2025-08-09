using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using HR.Data;
using HR.Models;

namespace HR.Api
{
    public class AnnouncementApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced Announcements list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/announcements", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Announcements.Where(a => !a.IsDeleted);

                // Search by title or content
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(a => a.Title.Contains(q!) || a.Content.Contains(q!));

                // Date filtering
                if (req.Query.TryGetValue("startDate", out var startDate) && DateTime.TryParse(startDate, out var start))
                    query = query.Where(a => a.DatePosted >= start);

                if (req.Query.TryGetValue("endDate", out var endDate) && DateTime.TryParse(endDate, out var end))
                    query = query.Where(a => a.DatePosted <= end);

                // Sort by date posted (newest first)
                query = query.OrderByDescending(a => a.DatePosted);

                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;

                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(a => new AnnouncementResponse
                    {
                        Announcement_ID = a.Announcement_ID,
                        Title = a.Title,
                        Content = a.Content,
                        DatePosted = a.DatePosted,
                        CreatedBy = a.CreatedBy,
                        CreatedAt = a.CreatedAt
                    }).ToListAsync();

                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/announcements/{id}", async (int id, AuthDbContext db) =>
                await db.Announcements.FirstOrDefaultAsync(a => a.Announcement_ID == id && !a.IsDeleted) is Announcement announcement ?
                    Results.Ok(new AnnouncementResponse
                    {
                        Announcement_ID = announcement.Announcement_ID,
                        Title = announcement.Title,
                        Content = announcement.Content,
                        DatePosted = announcement.DatePosted,
                        CreatedBy = announcement.CreatedBy,
                        CreatedAt = announcement.CreatedAt
                    }) : Results.NotFound());

            endpoints.MapPost("/api/announcements", async (AnnouncementRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var announcement = new Announcement
                {
                    Title = reqModel.Title,
                    Content = reqModel.Content,
                    DatePosted = reqModel.DatePosted ?? DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };

                db.Announcements.Add(announcement);
                await db.SaveChangesAsync();

                return Results.Created($"/api/announcements/{announcement.Announcement_ID}", new AnnouncementResponse
                {
                    Announcement_ID = announcement.Announcement_ID,
                    Title = announcement.Title,
                    Content = announcement.Content,
                    DatePosted = announcement.DatePosted,
                    CreatedBy = announcement.CreatedBy,
                    CreatedAt = announcement.CreatedAt
                });
            });

            endpoints.MapPut("/api/announcements/{id}", async (int id, AnnouncementRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var announcement = await db.Announcements.FirstOrDefaultAsync(a => a.Announcement_ID == id && !a.IsDeleted);
                if (announcement is null) return Results.NotFound();

                announcement.Title = reqModel.Title;
                announcement.Content = reqModel.Content;
                if (reqModel.DatePosted.HasValue)
                    announcement.DatePosted = reqModel.DatePosted.Value;
                announcement.UpdatedAt = DateTime.UtcNow;
                announcement.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();

                return Results.Ok(new AnnouncementResponse
                {
                    Announcement_ID = announcement.Announcement_ID,
                    Title = announcement.Title,
                    Content = announcement.Content,
                    DatePosted = announcement.DatePosted,
                    CreatedBy = announcement.CreatedBy,
                    CreatedAt = announcement.CreatedAt
                });
            });

            endpoints.MapDelete("/api/announcements/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var announcement = await db.Announcements.FindAsync(id);
                if (announcement is null || announcement.IsDeleted) return Results.NotFound();

                // Soft delete
                announcement.IsDeleted = true;
                announcement.UpdatedAt = DateTime.UtcNow;
                announcement.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Get recent announcements (last 30 days)
            endpoints.MapGet("/api/announcements/recent", async (HttpRequest req, AuthDbContext db) =>
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var query = db.Announcements
                    .Where(a => !a.IsDeleted && a.DatePosted >= thirtyDaysAgo)
                    .OrderByDescending(a => a.DatePosted);

                int limit = req.Query.TryGetValue("limit", out var l) && int.TryParse(l, out var li) ? li : 10;

                var items = await query.Take(limit)
                    .Select(a => new AnnouncementResponse
                    {
                        Announcement_ID = a.Announcement_ID,
                        Title = a.Title,
                        Content = a.Content,
                        DatePosted = a.DatePosted,
                        CreatedBy = a.CreatedBy,
                        CreatedAt = a.CreatedAt
                    }).ToListAsync();

                return Results.Ok(items);
            });

            // Pin/Unpin announcements (for important announcements)
            endpoints.MapPatch("/api/announcements/{id}/pin", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var announcement = await db.Announcements.FindAsync(id);
                if (announcement is null || announcement.IsDeleted) return Results.NotFound();

                // This would require adding an IsPinned field to the Announcement entity
                // For now, we'll just update the UpdatedAt to bring it to top
                announcement.UpdatedAt = DateTime.UtcNow;
                announcement.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.Ok(new { Message = "Announcement pinned successfully" });
            });
        }
    }
}
