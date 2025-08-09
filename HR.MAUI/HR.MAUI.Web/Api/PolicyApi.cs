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
                        PolicyNumber = p.PolicyNumber,
                        Category = p.Category,
                        Status = p.Status,
                        Version = p.Version,
                        EffectiveDate = p.EffectiveDate,
                        NextReviewDate = p.NextReviewDate,
                        Summary = p.Summary,
                        Content = p.Content,
                        ApprovalNotes = p.ApprovalNotes,
                        CreatedAt = p.CreatedAt,
                        LastUpdated = p.UpdatedAt,
                        CreatedBy = p.CreatedBy,
                        UpdatedBy = p.UpdatedBy,
                        Priority = p.Priority
                    }).ToListAsync();
                return Results.Ok(new PolicyListResponse { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            endpoints.MapGet("/api/policies/{id}", async (int id, AuthDbContext db) =>
                await db.Policies.FindAsync(id) is Policy p ?
                    Results.Ok(new PolicyResponse
                    {
                        Policy_ID = p.Policy_ID,
                        Title = p.Title,
                        PolicyNumber = p.PolicyNumber,
                        Category = p.Category,
                        Status = p.Status,
                        Version = p.Version,
                        EffectiveDate = p.EffectiveDate,
                        NextReviewDate = p.NextReviewDate,
                        Summary = p.Summary,
                        Content = p.Content,
                        ApprovalNotes = p.ApprovalNotes,
                        CreatedAt = p.CreatedAt,
                        LastUpdated = p.UpdatedAt,
                        CreatedBy = p.CreatedBy,
                        UpdatedBy = p.UpdatedBy,
                        Priority = p.Priority
                    }) : Results.NotFound());
            endpoints.MapPost("/api/policies", async (PolicyRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var policy = new Policy
                {
                    Title = reqModel.Title,
                    PolicyNumber = reqModel.PolicyNumber,
                    Category = reqModel.Category,
                    Status = reqModel.Status,
                    Version = reqModel.Version,
                    EffectiveDate = reqModel.EffectiveDate,
                    NextReviewDate = reqModel.NextReviewDate,
                    Summary = reqModel.Summary,
                    Content = reqModel.Content,
                    ApprovalNotes = reqModel.ApprovalNotes,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name,
                    Priority = "Medium"
                };
                db.Policies.Add(policy);
                await db.SaveChangesAsync();
                return Results.Created($"/api/policies/{policy.Policy_ID}", new PolicyResponse
                {
                    Policy_ID = policy.Policy_ID,
                    Title = policy.Title,
                    PolicyNumber = policy.PolicyNumber,
                    Category = policy.Category,
                    Status = policy.Status,
                    Version = policy.Version,
                    EffectiveDate = policy.EffectiveDate,
                    NextReviewDate = policy.NextReviewDate,
                    Summary = policy.Summary,
                    Content = policy.Content,
                    ApprovalNotes = policy.ApprovalNotes,
                    CreatedAt = policy.CreatedAt,
                    LastUpdated = policy.UpdatedAt,
                    CreatedBy = policy.CreatedBy,
                    UpdatedBy = policy.UpdatedBy,
                    Priority = policy.Priority
                });
            });
            endpoints.MapPut("/api/policies/{id}", async (int id, PolicyRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var p = await db.Policies.FindAsync(id);
                if (p is null) return Results.NotFound();
                p.Title = reqModel.Title;
                p.PolicyNumber = reqModel.PolicyNumber;
                p.Category = reqModel.Category;
                p.Status = reqModel.Status;
                p.Version = reqModel.Version;
                p.EffectiveDate = reqModel.EffectiveDate;
                p.NextReviewDate = reqModel.NextReviewDate;
                p.Summary = reqModel.Summary;
                p.Content = reqModel.Content;
                p.ApprovalNotes = reqModel.ApprovalNotes;
                p.UpdatedAt = DateTime.UtcNow;
                p.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(new PolicyResponse
                {
                    Policy_ID = p.Policy_ID,
                    Title = p.Title,
                    PolicyNumber = p.PolicyNumber,
                    Category = p.Category,
                    Status = p.Status,
                    Version = p.Version,
                    EffectiveDate = p.EffectiveDate,
                    NextReviewDate = p.NextReviewDate,
                    Summary = p.Summary,
                    Content = p.Content,
                    ApprovalNotes = p.ApprovalNotes,
                    CreatedAt = p.CreatedAt,
                    LastUpdated = p.UpdatedAt,
                    CreatedBy = p.CreatedBy,
                    UpdatedBy = p.UpdatedBy,
                    Priority = p.Priority
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