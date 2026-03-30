using AutoChecker.Interfaces;
using AutoChecker.Models;

namespace AutoChecker.Services.Validators.Optional;

public class TimeFormatValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.StartTime > x.EndTime)
            .Select(x => {
                var error = ValidationResultFactory.Hard($"[Time Error] Invalid time range for {x.TechnicianName}");
                x.ValidationResult.Add(error);
                return error;
            })];
    }
}
