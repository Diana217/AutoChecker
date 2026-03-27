using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class ServiceSiteWorkingHoursValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .Where(x => context.ServiceSites.ContainsKey((x.LocationName, x.ActivityType)))
            .Where(x =>
            {
                var site = context.ServiceSites[(x.LocationName, x.ActivityType)];

                if (!site.WorkingHours.TryGetValue(x.Date.DayOfWeek, out var hours))
                    return true;

                return x.StartTime < hours.From || x.EndTime > hours.To;
            })
            .Select(x => ValidationResultFactory
                .Hard($"[Site Closed] {x.LocationName} ({x.ActivityType}) is closed at {x.StartTime}-{x.EndTime} for {x.TechnicianName}"))];
    }
}