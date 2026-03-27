using AutoChecker.Interfaces;
using AutoChecker.Models;

namespace AutoChecker.Services.Validators;

public class BreakValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => context.Technicians.ContainsKey(x.TechnicianName))
            .GroupBy(x => (x.TechnicianName, x.Date))
            .SelectMany(g =>
            {
                var tech = context.Technicians[g.Key.TechnicianName];
                var visits = g.OrderBy(x => x.StartTime).ToList();

                var errors = new List<ValidationResult>();

                for (int i = 1; i < visits.Count; i++)
                {
                    var gap = visits[i].StartTime - visits[i - 1].EndTime;

                    if (gap.TotalMinutes >= tech.MinBreakMinutes)
                    {
                        var breakStart = visits[i - 1].EndTime;

                        if (breakStart < tech.BreakNotEarlierThan ||
                            breakStart > tech.BreakNotLaterThan)
                        {
                            errors.Add(ValidationResultFactory
                                .Soft($"[Break Window] {g.Key.TechnicianName} has break outside allowed window"));
                        }

                        return errors;
                    }
                }

                errors.Add(ValidationResultFactory.Hard($"[Break Missing] {g.Key.TechnicianName} has no valid break"));

                return errors;
            })];
    }
}
