using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using HR.Data;
using HR.Models;

namespace HR.Api
{
    public class AwardApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/awards", async (HttpRequest req, AuthDbContext db) =>
            {
                var list = await db.Awards.ToListAsync();
                return Results.Ok(list.Select(a => new AwardResponse
                {
                    Award_ID = a.Award_ID,
                    Title = a.Title,
                    Description = a.Description,
                    DateAwarded = a.DateAwarded,
                    Employee_ID = a.Employee_ID
                }));
            });
            endpoints.MapGet("/api/awards/{id}", async (int id, AuthDbContext db) =>
                await db.Awards.FindAsync(id) is Award a ?
                    Results.Ok(new AwardResponse
                    {
                        Award_ID = a.Award_ID,
                        Title = a.Title,
                        Description = a.Description,
                        DateAwarded = a.DateAwarded,
                        Employee_ID = a.Employee_ID
                    }) : Results.NotFound());
            endpoints.MapPost("/api/awards", async (AwardRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var award = new Award
                {
                    Title = reqModel.Title,
                    Employee_ID = reqModel.Employee_ID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Awards.Add(award);
                await db.SaveChangesAsync();
                return Results.Created($"/api/awards/{award.Award_ID}", new AwardResponse
                {
                    Award_ID = award.Award_ID,
                    Title = award.Title,
                    Description = award.Description,
                    DateAwarded = award.DateAwarded,
                    Employee_ID = award.Employee_ID
                });
            });
            endpoints.MapPut("/api/awards/{id}", async (int id, AwardRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var award = await db.Awards.FindAsync(id);
                if (award is null) return Results.NotFound();
                award.Title = reqModel.Title;
                award.Employee_ID = reqModel.Employee_ID;
                award.UpdatedAt = DateTime.UtcNow;
                award.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(new AwardResponse
                {
                    Award_ID = award.Award_ID,
                    Title = award.Title,
                    Description = award.Description,
                    DateAwarded = award.DateAwarded,
                    Employee_ID = award.Employee_ID
                });
            });
            endpoints.MapDelete("/api/awards/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var award = await db.Awards.FindAsync(id);
                if (award is null) return Results.NotFound();
                db.Awards.Remove(award);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
