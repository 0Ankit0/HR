using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

namespace HR.Api
{
    public class EmployeeTrainingApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/employeetrainings", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Employee_Trainings.Include(et => et.Employee).Include(et => et.Training).Where(et => !et.IsDeleted);
                
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(et => et.Employee_ID == eid);
                
                // Filtering by training
                if (req.Query.TryGetValue("trainingId", out var tid) && int.TryParse(tid, out var tidInt))
                    query = query.Where(et => et.Training_ID == tidInt);
                
                // Filter by completion status
                if (req.Query.TryGetValue("completed", out var completed) && bool.TryParse(completed, out var isCompleted))
                {
                    if (isCompleted)
                        query = query.Where(et => et.Completion_Date.HasValue);
                    else
                        query = query.Where(et => !et.Completion_Date.HasValue);
                }
                
                // Filter by score range
                if (req.Query.TryGetValue("minScore", out var minScore) && double.TryParse(minScore, out var min))
                    query = query.Where(et => et.Score >= min);
                
                if (req.Query.TryGetValue("maxScore", out var maxScore) && double.TryParse(maxScore, out var max))
                    query = query.Where(et => et.Score <= max);
                
                // Date filtering for completion
                if (req.Query.TryGetValue("completedAfter", out var completedAfter) && DateTime.TryParse(completedAfter, out var after))
                    query = query.Where(et => et.Completion_Date >= after);
                
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(et => new EmployeeTrainingResponse
                    {
                        Employee_Training_ID = et.Employee_Training_ID,
                        Employee_ID = et.Employee_ID,
                        EmployeeName = et.Employee != null ? et.Employee.Name : null,
                        Training_ID = et.Training_ID,
                        TrainingTitle = et.Training != null ? et.Training.Title : null,
                        Completion_Date = et.Completion_Date,
                        Score = et.Score,
                        Status = et.Completion_Date.HasValue ? "Completed" : "In Progress",
                        AssignedDate = et.CreatedAt
                    }).ToListAsync();
                
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/employeetrainings/{id}", async (int id, AuthDbContext db) =>
                await db.Employee_Trainings.Include(et => et.Employee).Include(et => et.Training)
                    .FirstOrDefaultAsync(et => et.Employee_Training_ID == id && !et.IsDeleted) is Employee_Training et ?
                    Results.Ok(new EmployeeTrainingResponse
                    {
                        Employee_Training_ID = et.Employee_Training_ID,
                        Employee_ID = et.Employee_ID,
                        EmployeeName = et.Employee?.Name,
                        Training_ID = et.Training_ID,
                        TrainingTitle = et.Training?.Title,
                        Completion_Date = et.Completion_Date,
                        Score = et.Score,
                        Status = et.Completion_Date.HasValue ? "Completed" : "In Progress",
                        AssignedDate = et.CreatedAt
                    }) : Results.NotFound());

            endpoints.MapPost("/api/employeetrainings", async (EmployeeTrainingRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                // Validate employee and training exist
                var employeeExists = await db.Employees.AnyAsync(e => e.Employee_ID == reqModel.Employee_ID && !e.IsDeleted);
                if (!employeeExists)
                    return Results.BadRequest("Employee not found");

                var trainingExists = await db.Trainings.AnyAsync(t => t.Training_ID == reqModel.Training_ID && !t.IsDeleted);
                if (!trainingExists)
                    return Results.BadRequest("Training not found");

                // Check if assignment already exists
                var existingAssignment = await db.Employee_Trainings
                    .AnyAsync(et => et.Employee_ID == reqModel.Employee_ID && et.Training_ID == reqModel.Training_ID && !et.IsDeleted);
                if (existingAssignment)
                    return Results.BadRequest("Employee is already assigned to this training");

                var et = new Employee_Training
                {
                    Employee_ID = reqModel.Employee_ID,
                    Training_ID = reqModel.Training_ID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                
                db.Employee_Trainings.Add(et);
                await db.SaveChangesAsync();
                
                // Load related data for response
                await db.Entry(et).Reference(e => e.Employee).LoadAsync();
                await db.Entry(et).Reference(e => e.Training).LoadAsync();
                
                var response = new EmployeeTrainingResponse
                {
                    Employee_Training_ID = et.Employee_Training_ID,
                    Employee_ID = et.Employee_ID,
                    EmployeeName = et.Employee?.Name,
                    Training_ID = et.Training_ID,
                    TrainingTitle = et.Training?.Title,
                    Completion_Date = et.Completion_Date,
                    Score = et.Score,
                    Status = "In Progress",
                    AssignedDate = et.CreatedAt
                };
                
                return Results.Created($"/api/employeetrainings/{et.Employee_Training_ID}", response);
            });

            endpoints.MapPut("/api/employeetrainings/{id}", async (int id, EmployeeTrainingRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var et = await db.Employee_Trainings.Include(e => e.Employee).Include(e => e.Training)
                    .FirstOrDefaultAsync(e => e.Employee_Training_ID == id && !e.IsDeleted);
                if (et is null) return Results.NotFound();

                // Validate employee and training exist if changing them
                if (et.Employee_ID != reqModel.Employee_ID)
                {
                    var employeeExists = await db.Employees.AnyAsync(e => e.Employee_ID == reqModel.Employee_ID && !e.IsDeleted);
                    if (!employeeExists)
                        return Results.BadRequest("Employee not found");
                }

                if (et.Training_ID != reqModel.Training_ID)
                {
                    var trainingExists = await db.Trainings.AnyAsync(t => t.Training_ID == reqModel.Training_ID && !t.IsDeleted);
                    if (!trainingExists)
                        return Results.BadRequest("Training not found");
                }

                et.Employee_ID = reqModel.Employee_ID;
                et.Training_ID = reqModel.Training_ID;
                et.UpdatedAt = DateTime.UtcNow;
                et.UpdatedBy = ctx.User?.Identity?.Name;
                
                await db.SaveChangesAsync();
                
                var response = new EmployeeTrainingResponse
                {
                    Employee_Training_ID = et.Employee_Training_ID,
                    Employee_ID = et.Employee_ID,
                    EmployeeName = et.Employee?.Name,
                    Training_ID = et.Training_ID,
                    TrainingTitle = et.Training?.Title,
                    Completion_Date = et.Completion_Date,
                    Score = et.Score,
                    Status = et.Completion_Date.HasValue ? "Completed" : "In Progress",
                    AssignedDate = et.CreatedAt
                };
                
                return Results.Ok(response);
            });

            // Mark training as completed with score
            endpoints.MapPatch("/api/employeetrainings/{id}/complete", async (int id, EmployeeTrainingCompletionRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var et = await db.Employee_Trainings.Include(e => e.Employee).Include(e => e.Training)
                    .FirstOrDefaultAsync(e => e.Employee_Training_ID == id && !e.IsDeleted);
                if (et is null) return Results.NotFound();

                if (et.Completion_Date.HasValue)
                    return Results.BadRequest("Training is already completed");

                et.Completion_Date = reqModel.CompletionDate ?? DateTime.UtcNow;
                et.Score = reqModel.Score;
                et.UpdatedAt = DateTime.UtcNow;
                et.UpdatedBy = ctx.User?.Identity?.Name;
                
                await db.SaveChangesAsync();
                
                return Results.Ok(new { Message = "Training marked as completed", CompletionDate = et.Completion_Date, Score = et.Score });
            });

            endpoints.MapDelete("/api/employeetrainings/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var et = await db.Employee_Trainings.FindAsync(id);
                if (et is null || et.IsDeleted) return Results.NotFound();
                
                // Soft delete
                et.IsDeleted = true;
                et.UpdatedAt = DateTime.UtcNow;
                et.UpdatedBy = ctx.User?.Identity?.Name;
                
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            // Get training progress for an employee
            endpoints.MapGet("/api/employeetrainings/employee/{employeeId}/progress", async (int employeeId, AuthDbContext db) =>
            {
                var assignments = await db.Employee_Trainings.Include(et => et.Training)
                    .Where(et => et.Employee_ID == employeeId && !et.IsDeleted)
                    .Select(et => new
                    {
                        TrainingId = et.Training_ID,
                        TrainingTitle = et.Training != null ? et.Training.Title : "Unknown",
                        AssignedDate = et.CreatedAt,
                        CompletionDate = et.Completion_Date,
                        Score = et.Score,
                        Status = et.Completion_Date.HasValue ? "Completed" : "In Progress"
                    }).ToListAsync();

                var summary = new
                {
                    TotalAssigned = assignments.Count,
                    Completed = assignments.Count(a => a.CompletionDate.HasValue),
                    InProgress = assignments.Count(a => !a.CompletionDate.HasValue),
                    AverageScore = assignments.Where(a => a.Score.HasValue).Select(a => a.Score!.Value).DefaultIfEmpty(0).Average(),
                    Trainings = assignments
                };

                return Results.Ok(summary);
            });

            // Bulk assign trainings to multiple employees
            endpoints.MapPost("/api/employeetrainings/bulk-assign", async (BulkEmployeeTrainingRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var assignments = new List<Employee_Training>();
                var errors = new List<string>();

                foreach (var employeeId in reqModel.EmployeeIds)
                {
                    // Validate employee exists
                    var employeeExists = await db.Employees.AnyAsync(e => e.Employee_ID == employeeId && !e.IsDeleted);
                    if (!employeeExists)
                    {
                        errors.Add($"Employee ID {employeeId} not found");
                        continue;
                    }

                    // Check if assignment already exists
                    var existingAssignment = await db.Employee_Trainings
                        .AnyAsync(et => et.Employee_ID == employeeId && et.Training_ID == reqModel.TrainingId && !et.IsDeleted);
                    if (existingAssignment)
                    {
                        errors.Add($"Employee ID {employeeId} is already assigned to this training");
                        continue;
                    }

                    assignments.Add(new Employee_Training
                    {
                        Employee_ID = employeeId,
                        Training_ID = reqModel.TrainingId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = ctx.User?.Identity?.Name
                    });
                }

                if (assignments.Any())
                {
                    db.Employee_Trainings.AddRange(assignments);
                    await db.SaveChangesAsync();
                }

                return Results.Ok(new { AssignmentsCreated = assignments.Count, Errors = errors });
            });
        }
    }
}