using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using HR.Data;
using HR.Models;

namespace HR.Api
{
    public class MessageApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced Messages list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/messages", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Messages.Include(m => m.Sender).Include(m => m.Recipient).Where(m => !m.IsDeleted);

                // Filter by sender
                if (req.Query.TryGetValue("senderId", out var senderId) && int.TryParse(senderId, out var sid))
                    query = query.Where(m => m.Sender_ID == sid);

                // Filter by recipient
                if (req.Query.TryGetValue("recipientId", out var recipientId) && int.TryParse(recipientId, out var rid))
                    query = query.Where(m => m.Recipient_ID == rid);

                // Filter by conversation (either sender or recipient)
                if (req.Query.TryGetValue("userId", out var userId) && int.TryParse(userId, out var uid))
                    query = query.Where(m => m.Sender_ID == uid || m.Recipient_ID == uid);

                // Filter by read status
                if (req.Query.TryGetValue("isRead", out var isRead) && bool.TryParse(isRead, out var read))
                    query = query.Where(m => m.IsRead == read);

                // Search by content
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(m => m.Content.Contains(q!));

                // Date filtering
                if (req.Query.TryGetValue("startDate", out var startDate) && DateTime.TryParse(startDate, out var start))
                    query = query.Where(m => m.SentAt >= start);

                if (req.Query.TryGetValue("endDate", out var endDate) && DateTime.TryParse(endDate, out var end))
                    query = query.Where(m => m.SentAt <= end);

                // Sort by sent date (newest first)
                query = query.OrderByDescending(m => m.SentAt);

                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;

                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(m => new MessageResponse
                    {
                        Message_ID = m.Message_ID,
                        Sender_ID = m.Sender_ID,
                        SenderName = m.Sender != null ? m.Sender.Name : null,
                        Recipient_ID = m.Recipient_ID,
                        RecipientName = m.Recipient != null ? m.Recipient.Name : null,
                        Content = m.Content,
                        SentAt = m.SentAt,
                        IsRead = m.IsRead
                    }).ToListAsync();

                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/messages/{id}", async (int id, AuthDbContext db) =>
                await db.Messages.Include(m => m.Sender).Include(m => m.Recipient)
                    .FirstOrDefaultAsync(m => m.Message_ID == id && !m.IsDeleted) is Message message ?
                    Results.Ok(new MessageResponse
                    {
                        Message_ID = message.Message_ID,
                        Sender_ID = message.Sender_ID,
                        SenderName = message.Sender?.Name,
                        Recipient_ID = message.Recipient_ID,
                        RecipientName = message.Recipient?.Name,
                        Content = message.Content,
                        SentAt = message.SentAt,
                        IsRead = message.IsRead
                    }) : Results.NotFound());

            endpoints.MapPost("/api/messages", async (MessageRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                // Validate sender and recipient exist
                var senderExists = await db.Employees.AnyAsync(e => e.Employee_ID == reqModel.Sender_ID && !e.IsDeleted);
                if (!senderExists)
                    return Results.BadRequest("Sender not found");

                var recipientExists = await db.Employees.AnyAsync(e => e.Employee_ID == reqModel.Recipient_ID && !e.IsDeleted);
                if (!recipientExists)
                    return Results.BadRequest("Recipient not found");

                var message = new Message
                {
                    Sender_ID = reqModel.Sender_ID,
                    Recipient_ID = reqModel.Recipient_ID,
                    Content = reqModel.Content,
                    SentAt = DateTime.UtcNow,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };

                db.Messages.Add(message);
                await db.SaveChangesAsync();

                // Load related data for response
                await db.Entry(message).Reference(m => m.Sender).LoadAsync();
                await db.Entry(message).Reference(m => m.Recipient).LoadAsync();

                return Results.Created($"/api/messages/{message.Message_ID}", new MessageResponse
                {
                    Message_ID = message.Message_ID,
                    Sender_ID = message.Sender_ID,
                    SenderName = message.Sender?.Name,
                    Recipient_ID = message.Recipient_ID,
                    RecipientName = message.Recipient?.Name,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsRead = message.IsRead
                });
            });

            endpoints.MapPut("/api/messages/{id}", async (int id, MessageRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var message = await db.Messages.Include(m => m.Sender).Include(m => m.Recipient)
                    .FirstOrDefaultAsync(m => m.Message_ID == id && !m.IsDeleted);
                if (message is null) return Results.NotFound();

                // Only allow updating content and only by the sender
                message.Content = reqModel.Content;
                message.UpdatedAt = DateTime.UtcNow;
                message.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();

                return Results.Ok(new MessageResponse
                {
                    Message_ID = message.Message_ID,
                    Sender_ID = message.Sender_ID,
                    SenderName = message.Sender?.Name,
                    Recipient_ID = message.Recipient_ID,
                    RecipientName = message.Recipient?.Name,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsRead = message.IsRead
                });
            });

            // Mark message as read
            endpoints.MapPatch("/api/messages/{id}/read", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var message = await db.Messages.FindAsync(id);
                if (message is null || message.IsDeleted) return Results.NotFound();

                message.IsRead = true;
                message.UpdatedAt = DateTime.UtcNow;
                message.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.Ok(new { Message = "Message marked as read" });
            });

            // Mark message as unread
            endpoints.MapPatch("/api/messages/{id}/unread", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var message = await db.Messages.FindAsync(id);
                if (message is null || message.IsDeleted) return Results.NotFound();

                message.IsRead = false;
                message.UpdatedAt = DateTime.UtcNow;
                message.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.Ok(new { Message = "Message marked as unread" });
            });

            endpoints.MapDelete("/api/messages/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var message = await db.Messages.FindAsync(id);
                if (message is null || message.IsDeleted) return Results.NotFound();

                // Soft delete
                message.IsDeleted = true;
                message.UpdatedAt = DateTime.UtcNow;
                message.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Get conversation between two users
            endpoints.MapGet("/api/messages/conversation/{userId1}/{userId2}", async (int userId1, int userId2, HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Messages.Include(m => m.Sender).Include(m => m.Recipient)
                    .Where(m => !m.IsDeleted &&
                               ((m.Sender_ID == userId1 && m.Recipient_ID == userId2) ||
                                (m.Sender_ID == userId2 && m.Recipient_ID == userId1)))
                    .OrderBy(m => m.SentAt);

                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 50;

                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(m => new MessageResponse
                    {
                        Message_ID = m.Message_ID,
                        Sender_ID = m.Sender_ID,
                        SenderName = m.Sender != null ? m.Sender.Name : null,
                        Recipient_ID = m.Recipient_ID,
                        RecipientName = m.Recipient != null ? m.Recipient.Name : null,
                        Content = m.Content,
                        SentAt = m.SentAt,
                        IsRead = m.IsRead
                    }).ToListAsync();

                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            // Get unread message count for a user
            endpoints.MapGet("/api/messages/unread-count/{userId}", async (int userId, AuthDbContext db) =>
            {
                var count = await db.Messages
                    .CountAsync(m => !m.IsDeleted && m.Recipient_ID == userId && !m.IsRead);

                return Results.Ok(new { UnreadCount = count });
            });

            // Get user's inbox (received messages)
            endpoints.MapGet("/api/messages/inbox/{userId}", async (int userId, HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Messages.Include(m => m.Sender)
                    .Where(m => !m.IsDeleted && m.Recipient_ID == userId)
                    .OrderByDescending(m => m.SentAt);

                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;

                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(m => new MessageResponse
                    {
                        Message_ID = m.Message_ID,
                        Sender_ID = m.Sender_ID,
                        SenderName = m.Sender != null ? m.Sender.Name : null,
                        Recipient_ID = m.Recipient_ID,
                        Content = m.Content,
                        SentAt = m.SentAt,
                        IsRead = m.IsRead
                    }).ToListAsync();

                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            // Get user's sent messages
            endpoints.MapGet("/api/messages/sent/{userId}", async (int userId, HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Messages.Include(m => m.Recipient)
                    .Where(m => !m.IsDeleted && m.Sender_ID == userId)
                    .OrderByDescending(m => m.SentAt);

                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;

                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(m => new MessageResponse
                    {
                        Message_ID = m.Message_ID,
                        Sender_ID = m.Sender_ID,
                        Recipient_ID = m.Recipient_ID,
                        RecipientName = m.Recipient != null ? m.Recipient.Name : null,
                        Content = m.Content,
                        SentAt = m.SentAt,
                        IsRead = m.IsRead
                    }).ToListAsync();

                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
        }
    }
}
