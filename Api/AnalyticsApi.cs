using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using HR.Data;

namespace HR.Api
{
    public class AnalyticsApi : IApiEndpoint
    {
        public void MapApi(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/analytics");

            // Employee summary: total, by department, by job role
            group.MapGet("/employee-summary", async (AuthDbContext db) =>
            {
                var total = await db.Employees.CountAsync();
                var byDept = await db.Employees
                    .GroupBy(e => e.Department_ID)
                    .Select(g => new { Department_ID = g.Key, Count = g.Count() })
                    .ToListAsync();
                var byRole = await db.Employees
                    .GroupBy(e => e.JobRole_ID)
                    .Select(g => new { JobRole_ID = g.Key, Count = g.Count() })
                    .ToListAsync();
                return Results.Ok(new { TotalEmployees = total, ByDepartment = byDept, ByRole = byRole });
            });

            // Turnover analytics: hires vs. separations (using HireDate and optionally a TerminationDate if present)
            group.MapGet("/turnover", async (AuthDbContext db) =>
            {
                var currentYear = DateTime.UtcNow.Year;
                var hires = await db.Employees.CountAsync(e => e.HireDate.Year == currentYear);
                // If you add a TerminationDate property, use it here. For now, just return hires.
                return Results.Ok(new { Year = currentYear, Hires = hires, Separations = 0 });
            });

            // Diversity analytics: gender, department, etc. (assuming Gender property exists)
            group.MapGet("/diversity", async (AuthDbContext db) =>
            {
                // If Employee has Gender, use it. Otherwise, just by department for now.
                var byDept = await db.Employees
                    .GroupBy(e => e.Department_ID)
                    .Select(g => new { Department_ID = g.Key, Count = g.Count() })
                    .ToListAsync();
                return Results.Ok(new { ByDepartment = byDept });
            });

            // Absenteeism analytics: total absences per employee
            group.MapGet("/absenteeism", async (AuthDbContext db) =>
            {
                var absences = await db.Attendances
                    .Where(a => a.Status.ToLower() == "absent")
                    .GroupBy(a => a.Employee_ID)
                    .Select(g => new { Employee_ID = g.Key, AbsenceCount = g.Count() })
                    .ToListAsync();
                return Results.Ok(absences);
            });

            // Training participation: employees per training
            group.MapGet("/training-participation", async (AuthDbContext db) =>
            {
                var participation = await db.Employee_Trainings
                    .GroupBy(et => et.Training_ID)
                    .Select(g => new { Training_ID = g.Key, ParticipantCount = g.Count() })
                    .ToListAsync();
                return Results.Ok(participation);
            });

            // Performance review distribution
            group.MapGet("/performance-distribution", async (AuthDbContext db) =>
            {
                var distribution = await db.PerformanceReviews
                    .GroupBy(pr => pr.Score)
                    .Select(g => new { Score = g.Key, Count = g.Count() })
                    .ToListAsync();
                return Results.Ok(distribution);
            });

            // Compensation analysis: average salary by department
            group.MapGet("/compensation-by-department", async (AuthDbContext db) =>
            {
                var comp = await db.Employees
                    .Where(e => e.Payroll != null)
                    .GroupBy(e => e.Department_ID)
                    .Select(g => new
                    {
                        Department_ID = g.Key,
                        AvgSalary = g.Average(e => e.Payroll!.Salary)
                    })
                    .ToListAsync();
                return Results.Ok(comp);
            });

            // Headcount trend: hires per month for the current year
            group.MapGet("/headcount-trend", async (AuthDbContext db) =>
            {
                var currentYear = DateTime.UtcNow.Year;
                var hiresByMonth = await db.Employees
                    .Where(e => e.HireDate.Year == currentYear)
                    .GroupBy(e => e.HireDate.Month)
                    .Select(g => new { Month = g.Key, Hires = g.Count() })
                    .ToListAsync();
                return Results.Ok(hiresByMonth);
            });
        }
    }
}
