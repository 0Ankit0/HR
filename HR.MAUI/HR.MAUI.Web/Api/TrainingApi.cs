using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;
using System.Text.Json;

namespace HR.Api
{
    public class TrainingApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/api/trainings");

            // Enhanced list: filter, search, paging, exclude deleted
            group.MapGet("", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Trainings.Where(t => !t.IsDeleted);

                // Search by title
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(t => t.Title.Contains(q!));

                // Filter by status (using defaults since current model is limited)
                if (req.Query.TryGetValue("status", out var status) && !string.IsNullOrEmpty(status))
                {
                    // Default filtering logic based on dates since status not in current model
                    var now = DateTime.UtcNow;
                    if (string.Equals(status, "scheduled", StringComparison.OrdinalIgnoreCase))
                        query = query.Where(t => t.Date > now);
                    else if (string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase))
                        query = query.Where(t => t.Date < now);
                }

                // Filter by date range
                if (req.Query.TryGetValue("startDate", out var startDateStr) && DateTime.TryParse(startDateStr, out var startDate))
                    query = query.Where(t => t.Date >= startDate);

                if (req.Query.TryGetValue("endDate", out var endDateStr) && DateTime.TryParse(endDateStr, out var endDate))
                    query = query.Where(t => t.Date <= endDate);

                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;

                var total = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TrainingResponse
                    {
                        Training_ID = t.Training_ID,
                        Title = t.Title,
                        Description = "", // Default since not in current model
                        Date = t.Date,
                        EndDate = t.Date.AddHours(8), // Default 8-hour training
                        Instructor = "TBD", // Default since not in current model
                        Location = "TBD", // Default since not in current model
                        MaxAttendees = 50, // Default
                        CurrentAttendees = 0, // Would need Employee_Training relationship
                        Status = t.Date > DateTime.UtcNow ? "Scheduled" : "Completed",
                        TrainingType = "Classroom", // Default
                        Category = "General", // Default
                        Cost = 0, // Default
                        Prerequisites = "", // Default
                        LearningObjectives = "", // Default
                        IsMandatory = false, // Default
                        CreatedDate = t.CreatedAt,
                        LastModifiedDate = t.UpdatedAt,
                        EnrolledEmployees = new List<string>() // Would need Employee_Training relationship
                    }).ToListAsync();

                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            // Get single training
            group.MapGet("{id}", async (int id, AuthDbContext db) =>
            {
                var training = await db.Trainings.FirstOrDefaultAsync(t => t.Training_ID == id && !t.IsDeleted);
                if (training == null) return Results.NotFound();

                return Results.Ok(new TrainingResponse
                {
                    Training_ID = training.Training_ID,
                    Title = training.Title,
                    Description = "",
                    Date = training.Date,
                    EndDate = training.Date.AddHours(8),
                    Instructor = "TBD",
                    Location = "TBD",
                    MaxAttendees = 50,
                    CurrentAttendees = 0,
                    Status = training.Date > DateTime.UtcNow ? "Scheduled" : "Completed",
                    TrainingType = "Classroom",
                    Category = "General",
                    Cost = 0,
                    Prerequisites = "",
                    LearningObjectives = "",
                    IsMandatory = false,
                    CreatedDate = training.CreatedAt,
                    LastModifiedDate = training.UpdatedAt,
                    EnrolledEmployees = new List<string>()
                });
            });

            // Create training
            group.MapPost("", async (TrainingRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var training = new Training
                {
                    Title = reqModel.Title,
                    Date = reqModel.Date,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };

                db.Trainings.Add(training);
                await db.SaveChangesAsync();
                return Results.Created($"/api/trainings/{training.Training_ID}", training);
            });

            // Update training
            group.MapPut("{id}", async (int id, TrainingRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var training = await db.Trainings.FindAsync(id);
                if (training == null || training.IsDeleted) return Results.NotFound();

                training.Title = reqModel.Title;
                training.Date = reqModel.Date;
                training.UpdatedAt = DateTime.UtcNow;
                training.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.Ok(training);
            });

            // Update training status (using date-based logic)
            group.MapPatch("{id}/status", async (int id, JsonElement statusData, AuthDbContext db, HttpContext ctx) =>
            {
                var training = await db.Trainings.FindAsync(id);
                if (training == null || training.IsDeleted) return Results.NotFound();

                if (statusData.TryGetProperty("status", out var statusElement))
                {
                    var status = statusElement.GetString();
                    // For demo purposes, we can adjust the date to reflect status
                    if (status == "Completed")
                        training.Date = DateTime.UtcNow.AddDays(-1);
                    else if (status == "Scheduled")
                        training.Date = DateTime.UtcNow.AddDays(7);

                    training.UpdatedAt = DateTime.UtcNow;
                    training.UpdatedBy = ctx.User?.Identity?.Name;
                    await db.SaveChangesAsync();
                    return Results.Ok(new { training.Training_ID, Status = status });
                }

                return Results.BadRequest("Status is required");
            });

            // Delete training (soft delete)
            group.MapDelete("{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var training = await db.Trainings.FindAsync(id);
                if (training == null) return Results.NotFound();

                training.IsDeleted = true;
                training.UpdatedAt = DateTime.UtcNow;
                training.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok();
            });

            // Get training statistics
            group.MapGet("stats", async (AuthDbContext db) =>
            {
                var totalTrainings = await db.Trainings.CountAsync(t => !t.IsDeleted);
                var now = DateTime.UtcNow;
                var scheduledTrainings = await db.Trainings.CountAsync(t => !t.IsDeleted && t.Date > now);
                var completedTrainings = await db.Trainings.CountAsync(t => !t.IsDeleted && t.Date < now);

                var trainingByMonth = await db.Trainings
                    .Where(t => !t.IsDeleted && t.Date >= DateTime.Now.AddMonths(-12))
                    .GroupBy(t => new { t.Date.Year, t.Date.Month })
                    .Select(g => new TrainingByMonth
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Count = g.Count(),
                        TotalAttendees = g.Count() * 25, // Estimated
                        TotalCost = 0 // Default since cost not in current model
                    })
                    .ToListAsync();

                var stats = new TrainingStats
                {
                    TotalTrainings = totalTrainings,
                    ScheduledTrainings = scheduledTrainings,
                    CompletedTrainings = completedTrainings,
                    CancelledTrainings = 0, // Not tracked in current model
                    TotalAttendees = totalTrainings * 25, // Estimated
                    TotalCost = 0, // Not tracked in current model
                    TrainingByCategory = new List<TrainingByCategory>
                    {
                        new() { Category = "General", Count = totalTrainings, TotalAttendees = totalTrainings * 25, TotalCost = 0 }
                    },
                    TrainingByMonth = trainingByMonth
                };

                return Results.Ok(stats);
            });
        }
    }
}
