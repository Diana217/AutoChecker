using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class StartEndLocationValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .GroupBy(x => (x.TechnicianName, x.Date))
            .Where(g => context.Technicians.ContainsKey(g.Key.TechnicianName))
            .SelectMany(g =>
            {
                var tech = context.Technicians[g.Key.TechnicianName];
                var visits = g.OrderBy(x => x.StartTime).ToList();

                if (!visits.Any())
                    return [];

                var first = visits.First();
                var last = visits.Last();

                static bool Matches(string location, LocationType type) =>
                    string.Equals(location, type.ToString(), StringComparison.OrdinalIgnoreCase);

                var isValidStart =
                    tech.StartsFrom == LocationType.Either ||
                    (tech.StartsFrom == LocationType.Home && Matches(first.LocationName, LocationType.Home)) ||
                    (tech.StartsFrom == LocationType.Office && Matches(first.LocationName, LocationType.Office));

                var isValidEnd =
                    tech.FinishesAt == LocationType.Either ||
                    (tech.FinishesAt == LocationType.Home && Matches(last.LocationName, LocationType.Home)) ||
                    (tech.FinishesAt == LocationType.Office && Matches(last.LocationName, LocationType.Office));

                var results = new List<ValidationResult>();

                if (!isValidStart)
                {
                    results.Add(ValidationResultFactory.Soft(
                        $"[Start Location] {tech.Name} must start from {tech.StartsFrom}, but starts at {first.LocationName}"
                    ));
                }

                if (!isValidEnd)
                {
                    results.Add(ValidationResultFactory.Soft(
                        $"[End Location] {tech.Name} must finish at {tech.FinishesAt}, but ends at {last.LocationName}"
                    ));
                }

                return results;
            })];
    }
}
