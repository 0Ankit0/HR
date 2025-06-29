using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using HR.Data;
using HR.Api;
using HR.Models;

namespace HR.Api
{
    public class CommunicationApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/communication");

            // Enhanced Messages list: filter, search, paging, exclude deleted
            group.MapGet("/messages", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Messages.Include(m => m.Sender).Include(m => m.Recipient).Where(m => !m.IsDeleted);
                // Filtering by sender
                if (req.Query.TryGetValue("senderId", out var senderId) && int.TryParse(senderId, out var sid))
                    query = query.Where(m => m.Sender_ID == sid);
                // Filtering by recipient
                if (req.Query.TryGetValue("recipientId", out var recipientId) && int.TryParse(recipientId, out var rid))
                    query = query.Where(m => m.Recipient_ID == rid);
                // Search by content
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(m => m.Content.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(m => new MessageResponse
                    {
                        Message_ID = m.Message_ID,
                        Sender_ID = m.Sender_ID,
                        Recipient_ID = m.Recipient_ID,
                        Content = m.Content,
                        SentAt = m.SentAt,
                        IsRead = m.IsRead
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            group.MapGet("/messages/{id}", async (int id, AuthDbContext db) =>
                await db.Messages.Include(m => m.Sender).Include(m => m.Recipient).FirstOrDefaultAsync(m => m.Message_ID == id && !m.IsDeleted) is Message m ?
                    Results.Ok(new MessageResponse
                    {
                        Message_ID = m.Message_ID,
                        Sender_ID = m.Sender_ID,
                        Recipient_ID = m.Recipient_ID,
                        Content = m.Content,
                        SentAt = m.SentAt,
                        IsRead = m.IsRead
                    }) : Results.NotFound());
            group.MapPost("/messages", async (MessageRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var message = new Message
                {
                    Sender_ID = reqModel.Sender_ID,
                    Recipient_ID = reqModel.Recipient_ID,
                    Content = reqModel.Content,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Messages.Add(message);
                await db.SaveChangesAsync();
                var response = new MessageResponse
                {
                    Message_ID = message.Message_ID,
                    Sender_ID = message.Sender_ID,
                    Recipient_ID = message.Recipient_ID,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsRead = message.IsRead
                };
                return Results.Created($"/api/communication/messages/{message.Message_ID}", response);
            });
            group.MapPut("/messages/{id}", async (int id, MessageRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var message = await db.Messages.FindAsync(id);
                if (message is null) return Results.NotFound();
                message.Sender_ID = reqModel.Sender_ID;
                message.Recipient_ID = reqModel.Recipient_ID;
                message.Content = reqModel.Content;
                message.UpdatedAt = DateTime.UtcNow;
                message.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new MessageResponse
                {
                    Message_ID = message.Message_ID,
                    Sender_ID = message.Sender_ID,
                    Recipient_ID = message.Recipient_ID,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsRead = message.IsRead
                };
                return Results.Ok(response);
            });
            group.MapDelete("/messages/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var message = await db.Messages.FindAsync(id);
                if (message is null) return Results.NotFound();
                message.IsDeleted = true;
                message.UpdatedAt = DateTime.UtcNow;
                message.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
