using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class DurationValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .Where(x => context.ServiceSites.ContainsKey((x.LocationName, x.ActivityType)))
            .Where(x =>
            {
                var site = context.ServiceSites[(x.LocationName, x.ActivityType)];
                var duration = (x.EndTime - x.StartTime).TotalMinutes;

                return duration != site.DurationMinutes;
            })
            .Select(x =>
            {
                var site = context.ServiceSites[(x.LocationName, x.ActivityType)];
                var error = ValidationResultFactory.Hard(
                    $"[Duration] {x.LocationName} requires {site.DurationMinutes} min, got {(x.EndTime - x.StartTime).TotalMinutes}"
                );

                site.ValidationResults.Add(error);
                
                return error;
            })];
    }
}
