using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators.Optional;

public class NoConsecutiveVisitsValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.ServiceSites.Values
            .SelectMany(site =>
            {
                if (site.Frequency == null ||
                    site.Frequency.IsUnknown ||
                    site.Frequency.Times < 2)
                {
                    return [];
                }

                var visits = context.Visits
                    .Where(x =>
                        x.ActivityType != ActivityType.Commute &&
                        x.LocationName == site.Name &&
                        x.ActivityType == site.Service)
                    .OrderBy(x => x.Date)
                    .ToList();

                var results = new List<ValidationResult>();

                for (int i = 1; i < visits.Count; i++)
                {
                    var diff = (visits[i].Date - visits[i - 1].Date).TotalDays;

                    if (diff == 1)
                    {
                        results.Add(ValidationResultFactory.Soft(
                            $"[Consecutive] {site.Name} has visits on consecutive days ({visits[i - 1].Date:yyyy-MM-dd}, {visits[i].Date:yyyy-MM-dd})"
                        ));
                    }
                }

                return results;
            })];
    }
}
