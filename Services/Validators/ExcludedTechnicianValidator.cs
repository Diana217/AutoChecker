using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class ExcludedTechnicianValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .Where(x => context.ServiceSites.ContainsKey((x.LocationName, x.ActivityType)))
            .Where(x =>
            {
                var site = context.ServiceSites[(x.LocationName, x.ActivityType)];
                return site.ExcludedTechnicians.Contains(x.TechnicianName);
            })
            .Select(x =>
            {
                var error = ValidationResultFactory.Hard(
                    $"[Excluded] {x.TechnicianName} is not allowed at {x.LocationName}"
                );
                x.ValidationResults.Add(error);
                return error;
            })];
                
    }
}
