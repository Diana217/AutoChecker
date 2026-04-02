using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;
using System.Globalization;

namespace AutoChecker.Services.Validators;

public class MaxHoursPerWeekValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .GroupBy(x => (x.TechnicianName, Week: ISOWeek.GetWeekOfYear(x.Date)))
            .Where(g => context.Technicians.ContainsKey(g.Key.TechnicianName))
            .Where(g =>
            {
                var tech = context.Technicians[g.Key.TechnicianName];
                var total = g.Sum(x => (x.EndTime - x.StartTime).TotalHours);
                return total > tech.MaxHoursPerWeek;
            })
            .Select(g => {
                var error = ValidationResultFactory.Hard($"[Max Week Hours] {g.Key.TechnicianName} exceeds weekly limit");
                context.Technicians[g.Key.TechnicianName].ValidationResults.Add(error);
                return error;
            })];
    }
}
