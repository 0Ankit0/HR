using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;

namespace HR.Api
{
    public class JobRoleApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/jobroles", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.JobRoles.Where(r => !r.IsDeleted);

                // Search by name, description, or department
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(r => r.Role_Name.Contains(q!) ||
                                           r.Role_Description.Contains(q!) ||
                                           r.Department.Contains(q!));

                // Filter by department
                if (req.Query.TryGetValue("department", out var dept) && !string.IsNullOrEmpty(dept))
                    query = query.Where(r => r.Department == dept);

                // Filter by level
                if (req.Query.TryGetValue("level", out var level) && !string.IsNullOrEmpty(level))
                    query = query.Where(r => r.Level == level);

                // Filter by status
                if (req.Query.TryGetValue("status", out var status) && !string.IsNullOrEmpty(status))
                    query = query.Where(r => r.Status == status);

                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                var response = items.Select(r => new HR.Models.JobRoleResponse
                {
                    JobRole_ID = r.JobRole_ID,
                    Role_Name = r.Role_Name,
                    Role_Description = r.Role_Description,
                    Department = r.Department,
                    Level = r.Level,
                    Status = r.Status,
                    MinExperience = r.MinExperience,
                    SalaryRange = r.SalaryRange,
                    KeyResponsibilities = string.IsNullOrEmpty(r.KeyResponsibilities) ?
                        new List<string>() :
                        JsonSerializer.Deserialize<List<string>>(r.KeyResponsibilities) ?? new List<string>(),
                    RequiredSkills = string.IsNullOrEmpty(r.RequiredSkills) ?
                        new List<string>() :
                        JsonSerializer.Deserialize<List<string>>(r.RequiredSkills) ?? new List<string>(),
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    CreatedBy = r.CreatedBy,
                    UpdatedBy = r.UpdatedBy
                }).ToList();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = response });
            });

            endpoints.MapGet("/api/jobroles/{id}", async (int id, AuthDbContext db) =>
            {
                var r = await db.JobRoles.FindAsync(id);
                if (r == null || r.IsDeleted) return Results.NotFound();

                return Results.Ok(new HR.Models.JobRoleResponse
                {
                    JobRole_ID = r.JobRole_ID,
                    Role_Name = r.Role_Name,
                    Role_Description = r.Role_Description,
                    Department = r.Department,
                    Level = r.Level,
                    Status = r.Status,
                    MinExperience = r.MinExperience,
                    SalaryRange = r.SalaryRange,
                    KeyResponsibilities = string.IsNullOrEmpty(r.KeyResponsibilities) ?
                        new List<string>() :
                        JsonSerializer.Deserialize<List<string>>(r.KeyResponsibilities) ?? new List<string>(),
                    RequiredSkills = string.IsNullOrEmpty(r.RequiredSkills) ?
                        new List<string>() :
                        JsonSerializer.Deserialize<List<string>>(r.RequiredSkills) ?? new List<string>(),
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    CreatedBy = r.CreatedBy,
                    UpdatedBy = r.UpdatedBy
                });
            });

            endpoints.MapPost("/api/jobroles", async (HR.Models.JobRoleRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var jobRole = new JobRole
                {
                    Role_Name = reqModel.Role_Name,
                    Role_Description = reqModel.Role_Description,
                    Department = reqModel.Department,
                    Level = reqModel.Level,
                    Status = reqModel.Status,
                    MinExperience = reqModel.MinExperience,
                    SalaryRange = reqModel.SalaryRange,
                    KeyResponsibilities = JsonSerializer.Serialize(reqModel.KeyResponsibilities),
                    RequiredSkills = JsonSerializer.Serialize(reqModel.RequiredSkills),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.JobRoles.Add(jobRole);
                await db.SaveChangesAsync();
                return Results.Created($"/api/jobroles/{jobRole.JobRole_ID}", new HR.Models.JobRoleResponse
                {
                    JobRole_ID = jobRole.JobRole_ID,
                    Role_Name = jobRole.Role_Name,
                    Role_Description = jobRole.Role_Description,
                    Department = jobRole.Department,
                    Level = jobRole.Level,
                    Status = jobRole.Status,
                    MinExperience = jobRole.MinExperience,
                    SalaryRange = jobRole.SalaryRange,
                    KeyResponsibilities = reqModel.KeyResponsibilities,
                    RequiredSkills = reqModel.RequiredSkills,
                    CreatedAt = jobRole.CreatedAt,
                    UpdatedAt = jobRole.UpdatedAt,
                    CreatedBy = jobRole.CreatedBy,
                    UpdatedBy = jobRole.UpdatedBy
                });
            });

            endpoints.MapPut("/api/jobroles/{id}", async (int id, HR.Models.JobRoleRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var jobRole = await db.JobRoles.FindAsync(id);
                if (jobRole is null || jobRole.IsDeleted) return Results.NotFound();

                jobRole.Role_Name = reqModel.Role_Name;
                jobRole.Role_Description = reqModel.Role_Description;
                jobRole.Department = reqModel.Department;
                jobRole.Level = reqModel.Level;
                jobRole.Status = reqModel.Status;
                jobRole.MinExperience = reqModel.MinExperience;
                jobRole.SalaryRange = reqModel.SalaryRange;
                jobRole.KeyResponsibilities = JsonSerializer.Serialize(reqModel.KeyResponsibilities);
                jobRole.RequiredSkills = JsonSerializer.Serialize(reqModel.RequiredSkills);
                jobRole.UpdatedAt = DateTime.UtcNow;
                jobRole.UpdatedBy = ctx.User?.Identity?.Name;

                await db.SaveChangesAsync();
                return Results.Ok(new HR.Models.JobRoleResponse
                {
                    JobRole_ID = jobRole.JobRole_ID,
                    Role_Name = jobRole.Role_Name,
                    Role_Description = jobRole.Role_Description,
                    Department = jobRole.Department,
                    Level = jobRole.Level,
                    Status = jobRole.Status,
                    MinExperience = jobRole.MinExperience,
                    SalaryRange = jobRole.SalaryRange,
                    KeyResponsibilities = reqModel.KeyResponsibilities,
                    RequiredSkills = reqModel.RequiredSkills,
                    CreatedAt = jobRole.CreatedAt,
                    UpdatedAt = jobRole.UpdatedAt,
                    CreatedBy = jobRole.CreatedBy,
                    UpdatedBy = jobRole.UpdatedBy
                });
            });

            endpoints.MapDelete("/api/jobroles/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var jobRole = await db.JobRoles.FindAsync(id);
                if (jobRole is null || jobRole.IsDeleted) return Results.NotFound();

                jobRole.IsDeleted = true;
                jobRole.UpdatedAt = DateTime.UtcNow;
                jobRole.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
