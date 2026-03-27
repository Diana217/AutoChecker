using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class TechsNeededValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.ServiceSites.Values
            .SelectMany(site =>
            {
                var visits = context.Visits
                    .Where(x =>
                        x.ActivityType != ActivityType.Commute &&
                        x.LocationName == site.Name &&
                        x.ActivityType == site.Service)
                    .GroupBy(x => x.Date)
                    .SelectMany(g =>
                    {
                        var techs = g
                            .Select(x => x.TechnicianName)
                            .Distinct()
                            .ToList();

                        var actual = techs.Count;
                        var required = site.TechsNeeded;

                        if (actual == required)
                            return [];

                        if (actual < required)
                        {
                            return
                            [
                                ValidationResultFactory.Hard(
                                    $"[Techs Needed] {site.Name} on {g.Key:yyyy-MM-dd} requires {required} techs, but has {actual}"
                                )
                            ];
                        }

                        return new[]
                        {
                            ValidationResultFactory.Soft(
                                $"[Techs Needed] {site.Name} on {g.Key:yyyy-MM-dd} has {actual} techs, expected {required}"
                            )
                        };
                    });

                return visits;
            })];
    }
}
