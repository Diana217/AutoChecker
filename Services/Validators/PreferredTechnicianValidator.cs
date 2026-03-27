using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class PreferredTechnicianValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .Where(x => context.ServiceSites.ContainsKey((x.LocationName, x.ActivityType)))
            .SelectMany(x =>
            {
                var site = context.ServiceSites[(x.LocationName, x.ActivityType)];

                if (!site.PreferredTechnicians.Any())
                    return [];

                if (site.PreferredTechnicians.Contains(x.TechnicianName))
                    return [];

                return new[]
                {
                    ValidationResultFactory.Soft(
                        $"[Preferred] {x.TechnicianName} is not preferred for {x.LocationName}"
                    )
                };
            })];
    }
}
