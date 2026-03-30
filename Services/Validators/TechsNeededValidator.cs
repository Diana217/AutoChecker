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
                        ValidationResult error;
                        if (actual < required)
                        {
                            error = ValidationResultFactory.Hard(
                                    $"[Techs Needed] {site.Name} on {g.Key:yyyy-MM-dd} requires {required} techs, but has {actual}"
                                );
                            site.ValidationResults.Add(error);
                            return
                            [                                
                                error
                            ];
                        }
                        error = ValidationResultFactory.Soft(
                                $"[Techs Needed] {site.Name} on {g.Key:yyyy-MM-dd} has {actual} techs, more than required {required}"
                            );
                        site.ValidationResults.Add(error);
                        return new[]
                        {
                            error
                        };
                    });

                return visits;
            })];
    }
}
