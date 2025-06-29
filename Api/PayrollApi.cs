using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;

namespace HR.Api
{
    public class PayrollApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder endpoints)
        {
            // Enhanced list: filter, search, paging, exclude deleted
            endpoints.MapGet("/api/payrolls", async (HttpRequest req, AuthDbContext db) =>
            {
                var query = db.Payrolls.Where(p => !p.IsDeleted);
                // Filtering by employee
                if (req.Query.TryGetValue("employeeId", out var empId) && int.TryParse(empId, out var eid))
                    query = query.Where(p => p.Employee_ID == eid);
                // Paging
                int page = req.Query.TryGetValue("page", out var p) && int.TryParse(p, out var pi) ? pi : 1;
                int pageSize = req.Query.TryGetValue("pageSize", out var ps) && int.TryParse(ps, out var psi) ? psi : 20;
                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/payrolls/{id}", async (int id, AuthDbContext db) =>
                await db.Payrolls.FindAsync(id) is Payroll p ? Results.Ok(p) : Results.NotFound());

            endpoints.MapPost("/api/payrolls", async (Payroll payroll, AuthDbContext db, HttpContext ctx) =>
            {
                payroll.CreatedAt = DateTime.UtcNow;
                payroll.CreatedBy = ctx.User?.Identity?.Name;
                db.Payrolls.Add(payroll);
                await db.SaveChangesAsync();
                return Results.Created($"/api/payrolls/{payroll.Payroll_ID}", payroll);
            });
            endpoints.MapPut("/api/payrolls/{id}", async (int id, Payroll updated, AuthDbContext db, HttpContext ctx) =>
            {
                var payroll = await db.Payrolls.FindAsync(id);
                if (payroll is null) return Results.NotFound();
                payroll.Salary = updated.Salary;
                payroll.Payment_Date = updated.Payment_Date;
                payroll.Bonus = updated.Bonus;
                payroll.NetPay = updated.NetPay;
                payroll.Employee_ID = updated.Employee_ID;
                payroll.UpdatedAt = DateTime.UtcNow;
                payroll.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.Ok(payroll);
            });
            endpoints.MapDelete("/api/payrolls/{id}", async (int id, AuthDbContext db, HttpContext ctx) =>
            {
                var payroll = await db.Payrolls.FindAsync(id);
                if (payroll is null) return Results.NotFound();
                payroll.IsDeleted = true;
                payroll.UpdatedAt = DateTime.UtcNow;
                payroll.UpdatedBy = ctx.User?.Identity?.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }
    }
}
