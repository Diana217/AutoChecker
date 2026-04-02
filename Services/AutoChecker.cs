using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;
using AutoChecker.Services.Validators;
using AutoChecker.Services.Validators.Optional;

namespace AutoChecker.Services;

public class AutoChecker
{
    private readonly List<IValidator> _validators;

    public AutoChecker()
    {
        _validators =
        [
            new BreakValidator(),
            new CurrentTechnicianValidator(),
            new DurationValidator(),
            new ExcludedTechnicianValidator(),
            new MaxHoursPerDayValidator(),
            new MaxHoursPerWeekValidator(),
            new PermitValidator(),
            new PreferredTechnicianValidator(),
            new ServiceSiteWorkingHoursValidator(),
            new ServiceSkillLevelValidator(),
            new SkillValidator(),
            new StartEndLocationValidator(),
            new TechnicianWorkingHoursValidator(),
            new TechsNeededValidator(),
            new VisitFrequencyValidator(),

            new NoConsecutiveVisitsValidator(),
            new OverlappingVisitsValidator(),
            new TimeFormatValidator(),
            new WorkloadBalanceValidator()
        ];
    }

    public void Run(ValidationContext context)
    {
        var results = _validators
            .SelectMany(v => v.Validate(context))
            .ToList();

        var allErrors = new List<ValidationResult>();

        allErrors.AddRange(context.Technicians.Values.SelectMany(t => t.ValidationResults));

        allErrors.AddRange(context.ServiceSites.Values.SelectMany(s => s.ValidationResults));

        allErrors.AddRange(context.Visits.SelectMany(v => v.ValidationResults));
        
        allErrors = allErrors.Distinct().ToList();

        results = allErrors;

        var hardErrors = results.Where(x => x.Severity == ConstraintSeverity.Hard).ToList();
        var softErrors = results.Where(x => x.Severity == ConstraintSeverity.Soft).ToList();
        Console.WriteLine($"Total errors: {results.Count} (Hard: {hardErrors.Count}, Soft: {softErrors.Count})\n");
        if (hardErrors.Count > 0) 
        {
            Console.WriteLine("!!! HARD:");

            foreach (var error in hardErrors)
            {
                Console.WriteLine(error.Message);
            }
        }

        if (softErrors.Count > 0) 
        {
            Console.WriteLine("\n! SOFT:");

            foreach (var error in softErrors)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}
