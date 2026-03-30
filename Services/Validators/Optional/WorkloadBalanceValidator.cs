using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators.Optional;

public class WorkloadBalanceValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        var workloads = context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .GroupBy(x => x.TechnicianName)
            .Select(g => new
            {
                Tech = g.Key,
                Hours = g.Sum(x => (x.EndTime - x.StartTime).TotalHours)
            })
            .ToList();

        if (!workloads.Any())
            return [];

        var avg = workloads.Average(x => x.Hours);

        return [.. workloads
            .Where(x => x.Hours > avg * 1.5 || x.Hours < avg * 0.5)
            .Select(x => {
                var error = ValidationResultFactory.Soft(
                    $"[Workload] {x.Tech} has uneven workload ({x.Hours:F1}h vs avg {avg:F1}h)"
                );

                context.Technicians[x.Tech].ValidationResults.Add(error);
                return error;

            })];
    }
}
