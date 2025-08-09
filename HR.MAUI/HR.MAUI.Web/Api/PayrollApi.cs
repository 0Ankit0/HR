using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HR.Data;
using Microsoft.AspNetCore.Routing;
using HR.Models;

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
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                    .Select(p => new PayrollResponse
                    {
                        Payroll_ID = p.Payroll_ID,
                        Employee_ID = p.Employee_ID,
                        Salary = p.Salary,
                        PayDate = p.Payment_Date
                    }).ToListAsync();
                return Results.Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
            });

            endpoints.MapGet("/api/payrolls/{id}", async (int id, AuthDbContext db) =>
                await db.Payrolls.FindAsync(id) is Payroll p ?
                    Results.Ok(new PayrollResponse
                    {
                        Payroll_ID = p.Payroll_ID,
                        Employee_ID = p.Employee_ID,
                        Salary = p.Salary,
                        PayDate = p.Payment_Date
                    }) : Results.NotFound());

            endpoints.MapPost("/api/payrolls", async (PayrollRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var payroll = new Payroll
                {
                    Employee_ID = reqModel.Employee_ID,
                    Salary = reqModel.Salary,
                    Payment_Date = reqModel.PayDate
                };
                db.Payrolls.Add(payroll);
                await db.SaveChangesAsync();
                return Results.Created($"/api/payrolls/{payroll.Payroll_ID}", new PayrollResponse
                {
                    Payroll_ID = payroll.Payroll_ID,
                    Employee_ID = payroll.Employee_ID,
                    Salary = payroll.Salary,
                    PayDate = payroll.Payment_Date
                });
            });
            endpoints.MapPut("/api/payrolls/{id}", async (int id, PayrollRequest reqModel, AuthDbContext db, HttpContext ctx) =>
            {
                var payroll = await db.Payrolls.FindAsync(id);
                if (payroll is null) return Results.NotFound();
                payroll.Employee_ID = reqModel.Employee_ID;
                payroll.Salary = reqModel.Salary;
                payroll.Payment_Date = reqModel.PayDate;
                await db.SaveChangesAsync();
                return Results.Ok(new PayrollResponse
                {
                    Payroll_ID = payroll.Payroll_ID,
                    Employee_ID = payroll.Employee_ID,
                    Salary = payroll.Salary,
                    PayDate = payroll.Payment_Date
                });
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
