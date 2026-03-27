using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class PermitValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .Where(x => context.ServiceSites.ContainsKey((x.LocationName, x.ActivityType)))
            .SelectMany(x =>
            {
                var site = context.ServiceSites[(x.LocationName, x.ActivityType)];

                if (!site.PermitRequired)
                    return [];

                if (site.TechsWithPermit.Contains(x.TechnicianName))
                    return [];

                var severity = site.PermitDifficulty switch
                {
                    PermitDifficulty.Hard => ConstraintSeverity.Hard,
                    PermitDifficulty.Medium => ConstraintSeverity.Soft,
                    PermitDifficulty.Easy => ConstraintSeverity.Soft,
                    _ => ConstraintSeverity.Soft
                };

                return new[]
                {
                    severity == ConstraintSeverity.Hard
                        ? ValidationResultFactory.Hard(
                            $"[Permit] {x.TechnicianName} has no permit for {x.LocationName} (hard to obtain)"
                        )
                        : ValidationResultFactory.Soft(
                            $"[Permit] {x.TechnicianName} has no permit for {x.LocationName}"
                        )
                };
            })];
    }
}
