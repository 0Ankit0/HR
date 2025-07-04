using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

namespace HR.Api
{
    public class ProjectApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/projects", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Projects.Where(p => !p.IsDeleted);
                // Search by name
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(p => p.Project_Name.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(p => new ProjectResponse
                    {
                        Project_ID = p.Project_ID,
                        Project_Name = p.Project_Name,
                        Deadline = p.Deadline,
                        Budget = p.Budget
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/projects/{id}", async (int id, AuthDbContext db) =>
                await db.Projects.FindAsync(id) is Project p ?
                    Results.Ok(new ProjectResponse
                    {
                        Project_ID = p.Project_ID,
                        Project_Name = p.Project_Name,
                        Deadline = p.Deadline,
                        Budget = p.Budget
                    }) : Results.NotFound());

            endpoints.MapPost("/api/projects", async (ProjectRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var project = new Project
                {
                    Project_Name = reqModel.Project_Name,
                    Deadline = reqModel.Deadline,
                    Budget = reqModel.Budget
                };
                db.Projects.Add(project);
                await db.SaveChangesAsync();
                return Results.Created($"/api/projects/{project.Project_ID}", new ProjectResponse
                {
                    Project_ID = project.Project_ID,
                    Project_Name = project.Project_Name,
                    Deadline = project.Deadline,
                    Budget = project.Budget
                });
            });
            endpoints.MapPut("/api/projects/{id}", async (int id, ProjectRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var project = await db.Projects.FindAsync(id);
                if (project is null) return Results.NotFound();
                project.Project_Name = reqModel.Project_Name;
                project.Deadline = reqModel.Deadline;
                project.Budget = reqModel.Budget;
                await db.SaveChangesAsync();
                return Results.Ok(new ProjectResponse
                {
                    Project_ID = project.Project_ID,
                    Project_Name = project.Project_Name,
                    Deadline = project.Deadline,
                    Budget = project.Budget
                });
            });
            endpoints.MapDelete("/api/projects/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var project = await db.Projects.FindAsync(id);
                if (project is null) return Results.NotFound();
                project.IsDeleted = true;
                project.UpdatedAt = DateTime.UtcNow;
                project.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
