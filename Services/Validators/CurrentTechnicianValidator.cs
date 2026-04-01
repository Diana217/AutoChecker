using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class CurrentTechnicianValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .Where(x => context.ServiceSites.ContainsKey((x.LocationName, x.ActivityType)))
            .SelectMany(x =>
            {
                var site = context.ServiceSites[(x.LocationName, x.ActivityType)];

                if (string.IsNullOrEmpty(site.CurrentTechnician))
                    return [];

                if (site.CurrentTechnician == x.TechnicianName)
                    return [];
                
                var error = ValidationResultFactory.Soft(
                    $"[Current Tech] {x.TechnicianName} is not current technician for {x.LocationName}"
                );

                context.Technicians[x.TechnicianName].ValidationResults.Add(error);

                x.ValidationResults.Add(error);
                return new[]
                {                    
                    error
                };
            })];
    }
}
