using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

namespace HR.Api
{
    public class WellnessProgramApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/wellnessprograms", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.WellnessPrograms.Where(w => !w.IsDeleted);
                // Search by program name
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(w => w.ProgramName.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(w => new WellnessProgramResponse
                    {
                        WellnessProgram_ID = w.WellnessProgram_ID,
                        ProgramName = w.ProgramName,
                        Description = w.Description,
                        Category = w.Category,
                        ProgramType = w.ProgramType,
                        Status = w.Status,
                        StartDate = w.StartDate,
                        EndDate = w.EndDate,
                        Duration = w.Duration,
                        Location = w.Location,
                        MaxParticipants = w.MaxParticipants,
                        ParticipantCount = w.ParticipantCount,
                        Benefits = w.Benefits,
                        CreatedAt = w.CreatedAt,
                        UpdatedAt = w.UpdatedAt,
                        CreatedBy = w.CreatedBy,
                        UpdatedBy = w.UpdatedBy
                    }).ToListAsync();
                return Results.Ok(new WellnessProgramListResponse { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/wellnessprograms/{id}", async (int id, AuthDbContext db) =>
                await db.WellnessPrograms.FindAsync(id) is WellnessProgram w ?
                    Results.Ok(new WellnessProgramResponse
                    {
                        WellnessProgram_ID = w.WellnessProgram_ID,
                        ProgramName = w.ProgramName,
                        Description = w.Description,
                        Category = w.Category,
                        ProgramType = w.ProgramType,
                        Status = w.Status,
                        StartDate = w.StartDate,
                        EndDate = w.EndDate,
                        Duration = w.Duration,
                        Location = w.Location,
                        MaxParticipants = w.MaxParticipants,
                        ParticipantCount = w.ParticipantCount,
                        Benefits = w.Benefits,
                        CreatedAt = w.CreatedAt,
                        UpdatedAt = w.UpdatedAt,
                        CreatedBy = w.CreatedBy,
                        UpdatedBy = w.UpdatedBy
                    }) : Results.NotFound());

            endpoints.MapPost("/api/wellnessprograms", async (WellnessProgramRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var w = new WellnessProgram
                {
                    ProgramName = reqModel.ProgramName,
                    Description = reqModel.Description,
                    Category = reqModel.Category,
                    ProgramType = reqModel.ProgramType,
                    Status = reqModel.Status,
                    StartDate = reqModel.StartDate,
                    EndDate = reqModel.EndDate,
                    Duration = reqModel.Duration,
                    Location = reqModel.Location,
                    MaxParticipants = reqModel.MaxParticipants,
                    Benefits = reqModel.Benefits,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.WellnessPrograms.Add(w);
                await db.SaveChangesAsync();
                return Results.Created($"/api/wellnessprograms/{w.WellnessProgram_ID}", new WellnessProgramResponse
                {
                    WellnessProgram_ID = w.WellnessProgram_ID,
                    ProgramName = w.ProgramName,
                    Description = w.Description,
                    Category = w.Category,
                    ProgramType = w.ProgramType,
                    Status = w.Status,
                    StartDate = w.StartDate,
                    EndDate = w.EndDate,
                    Duration = w.Duration,
                    Location = w.Location,
                    MaxParticipants = w.MaxParticipants,
                    ParticipantCount = w.ParticipantCount,
                    Benefits = w.Benefits,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt,
                    CreatedBy = w.CreatedBy,
                    UpdatedBy = w.UpdatedBy
                });
            });
            endpoints.MapPut("/api/wellnessprograms/{id}", async (int id, WellnessProgramRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var w = await db.WellnessPrograms.FindAsync(id);
                if (w is null) return Results.NotFound();
                w.ProgramName = reqModel.ProgramName;
                w.Description = reqModel.Description;
                w.Category = reqModel.Category;
                w.ProgramType = reqModel.ProgramType;
                w.Status = reqModel.Status;
                w.StartDate = reqModel.StartDate;
                w.EndDate = reqModel.EndDate;
                w.Duration = reqModel.Duration;
                w.Location = reqModel.Location;
                w.MaxParticipants = reqModel.MaxParticipants;
                w.Benefits = reqModel.Benefits;
                w.UpdatedAt = DateTime.UtcNow;
                w.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(new WellnessProgramResponse
                {
                    WellnessProgram_ID = w.WellnessProgram_ID,
                    ProgramName = w.ProgramName,
                    Description = w.Description,
                    Category = w.Category,
                    ProgramType = w.ProgramType,
                    Status = w.Status,
                    StartDate = w.StartDate,
                    EndDate = w.EndDate,
                    Duration = w.Duration,
                    Location = w.Location,
                    MaxParticipants = w.MaxParticipants,
                    ParticipantCount = w.ParticipantCount,
                    Benefits = w.Benefits,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt,
                    CreatedBy = w.CreatedBy,
                    UpdatedBy = w.UpdatedBy
                });
            });
            endpoints.MapDelete("/api/wellnessprograms/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var w = await db.WellnessPrograms.FindAsync(id);
                if (w is null) return Results.NotFound();
                w.IsDeleted = true;
                w.UpdatedAt = DateTime.UtcNow;
                w.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}