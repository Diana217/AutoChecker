using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class VisitFrequencyValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.ServiceSites.Values
            .SelectMany(site =>
            {
                if (site.Frequency == null || site.Frequency.IsUnknown)
                    return Enumerable.Empty<ValidationResult>();

                var visits = context.Visits
                    .Where(x =>
                        x.ActivityType != ActivityType.Commute &&
                        x.LocationName == site.Name &&
                        x.ActivityType == site.Service)
                    .OrderBy(x => x.Date)
                    .ToList();

                if (!visits.Any())
                {
                    var error = ValidationResultFactory.Hard(
                            $"[Frequency] {site.Name} has no visits but requires {site.Frequency.Times} per {site.Frequency.DaysPeriod} days"
                        );
                    site.ValidationResults.Add(error);
                    return
                    [
                        error
                    ];
                }

                var errors = new List<ValidationResult>();

                var period = site.Frequency.DaysPeriod;
                var required = site.Frequency.Times;

                var startDate = visits.Min(x => x.Date).Date;
                var endDate = visits.Max(x => x.Date).Date;

                for (var windowStart = startDate; windowStart <= endDate; windowStart = windowStart.AddDays(period))
                {
                    var windowEnd = windowStart.AddDays(period);

                    var count = visits.Count(v =>
                        v.Date >= windowStart &&
                        v.Date < windowEnd);

                    if (count != required)
                    {   
                        var error = ValidationResultFactory.Hard(
                            $"[Frequency] {site.Name} requires {required} visits per {period} days, but got {count} between {windowStart:yyyy-MM-dd} and {windowEnd:yyyy-MM-dd}"
                        );
                        site.ValidationResults.Add(error);
                        errors.Add(error);
                    }
                }

                return errors;
            })];
    }
}