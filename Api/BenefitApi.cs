using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using HR.Data;
using HR.Models;

namespace HR.Api
{
    public class BenefitApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/benefits", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Benefits.Include(b => b.Employee).Where(b => !b.IsDeleted);
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(b => b.Employee_ID == eid);
                // Search by type
                if (req.Query.TryGetValue("q", out var q) && !string.IsNullOrEmpty(q))
                    query = query.Where(b => b.BenefitType.Contains(q!));
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(b => new BenefitResponse
                    {
                        Benefit_ID = b.Benefit_ID,
                        BenefitType = b.BenefitType,
                        Provider = b.Provider,
                        PolicyNumber = b.PolicyNumber,
                        EnrollmentDate = b.EnrollmentDate,
                        Employee_ID = b.Employee_ID
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });
            endpoints.MapGet("/api/benefits/{id}", async (int id, AuthDbContext db) =>
                await db.Benefits.Include(b => b.Employee).FirstOrDefaultAsync(b => b.Benefit_ID == id && !b.IsDeleted) is Benefit b
                    ? Results.Ok(new BenefitResponse
                    {
                        Benefit_ID = b.Benefit_ID,
                        BenefitType = b.BenefitType,
                        Provider = b.Provider,
                        PolicyNumber = b.PolicyNumber,
                        EnrollmentDate = b.EnrollmentDate,
                        Employee_ID = b.Employee_ID
                    })
                    : Results.NotFound());
            endpoints.MapPost("/api/benefits", async (BenefitRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var benefit = new Benefit
                {
                    BenefitType = reqModel.BenefitType,
                    Employee_ID = reqModel.Employee_ID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ctx.User?.Identity?.Name
                };
                db.Benefits.Add(benefit);
                await db.SaveChangesAsync();
                var response = new BenefitResponse
                {
                    Benefit_ID = benefit.Benefit_ID,
                    BenefitType = benefit.BenefitType,
                    Provider = benefit.Provider,
                    PolicyNumber = benefit.PolicyNumber,
                    EnrollmentDate = benefit.EnrollmentDate,
                    Employee_ID = benefit.Employee_ID
                };
                return Results.Created($"/api/benefits/{benefit.Benefit_ID}", response);
            });
            endpoints.MapPut("/api/benefits/{id}", async (int id, BenefitRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var benefit = await db.Benefits.FindAsync(id);
                if (benefit is null) return Results.NotFound();
                benefit.BenefitType = reqModel.BenefitType;
                benefit.Employee_ID = reqModel.Employee_ID;
                benefit.UpdatedAt = DateTime.UtcNow;
                benefit.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                var response = new BenefitResponse
                {
                    Benefit_ID = benefit.Benefit_ID,
                    BenefitType = benefit.BenefitType,
                    Provider = benefit.Provider,
                    PolicyNumber = benefit.PolicyNumber,
                    EnrollmentDate = benefit.EnrollmentDate,
                    Employee_ID = benefit.Employee_ID
                };
                return Results.Ok(response);
            });
            endpoints.MapDelete("/api/benefits/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var benefit = await db.Benefits.FindAsync(id);
                if (benefit is null) return Results.NotFound();
                benefit.IsDeleted = true;
                benefit.UpdatedAt = DateTime.UtcNow;
                benefit.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
