using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators.Optional;

public class OverlappingVisitsValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .GroupBy(x => (x.TechnicianName, x.Date))
            .SelectMany(group =>
            {
                var visits = group.OrderBy(x => x.StartTime).ToList();
                var errors = new List<ValidationResult>();

                for (int i = 1; i < visits.Count; i++)
                {
                    var prev = visits[i - 1];
                    var curr = visits[i];

                    if (curr.StartTime < prev.EndTime)
                    {
                        var error = ValidationResultFactory
                            .Hard($"[Overlap] {curr.TechnicianName} has overlapping visits at {curr.StartTime}");
                        curr.ValidationResults.Add(error);
                        errors.Add(error);
                    }
                }

                return errors;
            })];
    }
}
