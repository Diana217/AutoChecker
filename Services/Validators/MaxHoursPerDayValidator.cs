using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class MaxHoursPerDayValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .GroupBy(x => (x.TechnicianName, x.Date))
            .Where(g => context.Technicians.ContainsKey(g.Key.TechnicianName))
            .Where(g =>
            {
                var tech = context.Technicians[g.Key.TechnicianName];
                var total = g.Sum(x => (x.EndTime - x.StartTime).TotalHours);
                return total > tech.MaxHoursPerDay;
            })
            .Select(g => ValidationResultFactory.Hard($"[Max Day Hours] {g.Key.TechnicianName} exceeds daily limit"))];
    }
}
