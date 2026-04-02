using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class TechnicianWorkingHoursValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .Where(x => context.Technicians.ContainsKey(x.TechnicianName))
            .Where(x =>
            {
                var tech = context.Technicians[x.TechnicianName];

                if (!tech.WorkingHours.TryGetValue(x.Date.DayOfWeek, out var hours))
                    return true; 

                return x.StartTime < hours.From || x.EndTime > hours.To;
            })
            .Select(x => {
                var error = ValidationResultFactory
                .Hard($"[Tech Hours] {x.TechnicianName} works outside allowed hours at {x.StartTime}-{x.EndTime}");
                context.Technicians[x.TechnicianName].ValidationResults.Add(error);
                x.ValidationResults.Add(error);

                return error;
            })];
    }
}
