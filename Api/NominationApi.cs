using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using HR.Data;
using HR.Models;

namespace HR.Api
{
    public class NominationApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/nominations", async (HttpRequest req, AuthDbContext db) =>
            {
                var list = await db.Nominations.ToListAsync();
                return Results.Ok(list.Select(n => new NominationResponse
                {
                    Nomination_ID = n.Nomination_ID,
                    Employee_ID = n.Employee_ID,
                    Reason = n.Reason,
                    DateNominated = n.DateNominated,
                    IsAwarded = n.IsAwarded
                }));
            });
            endpoints.MapGet("/api/nominations/{id}", async (int id, AuthDbContext db) =>
                await db.Nominations.FindAsync(id) is Nomination n ?
                    Results.Ok(new NominationResponse
                    {
                        Nomination_ID = n.Nomination_ID,
                        Employee_ID = n.Employee_ID,
                        Reason = n.Reason,
                        DateNominated = n.DateNominated,
                        IsAwarded = n.IsAwarded
                    }) : Results.NotFound());
            endpoints.MapPost("/api/nominations", async (NominationRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var nomination = new Nomination
                {
                    Employee_ID = reqModel.Employee_ID,
                    Reason = reqModel.Reason,
                    DateNominated = DateTime.UtcNow,
                    IsAwarded = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Nominations.Add(nomination);
                await db.SaveChangesAsync();
                return Results.Created($"/api/nominations/{nomination.Nomination_ID}", new NominationResponse
                {
                    Nomination_ID = nomination.Nomination_ID,
                    Employee_ID = nomination.Employee_ID,
                    Reason = nomination.Reason,
                    DateNominated = nomination.DateNominated,
                    IsAwarded = nomination.IsAwarded
                });
            });
            endpoints.MapPut("/api/nominations/{id}", async (int id, NominationRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var nomination = await db.Nominations.FindAsync(id);
                if (nomination is null) return Results.NotFound();
                nomination.Employee_ID = reqModel.Employee_ID;
                nomination.Reason = reqModel.Reason;
                nomination.UpdatedAt = DateTime.UtcNow;
                nomination.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(new NominationResponse
                {
                    Nomination_ID = nomination.Nomination_ID,
                    Employee_ID = nomination.Employee_ID,
                    Reason = nomination.Reason,
                    DateNominated = nomination.DateNominated,
                    IsAwarded = nomination.IsAwarded
                });
            });
            endpoints.MapDelete("/api/nominations/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var nomination = await db.Nominations.FindAsync(id);
                if (nomination is null) return Results.NotFound();
                db.Nominations.Remove(nomination);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
