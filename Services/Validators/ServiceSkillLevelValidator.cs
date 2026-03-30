using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class ServiceSkillLevelValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .Where(x => context.Technicians.ContainsKey(x.TechnicianName))
            .Where(x => context.ServiceSites.ContainsKey((x.LocationName, x.ActivityType)))
            .SelectMany(x =>
            {
                var tech = context.Technicians[x.TechnicianName];
                var site = context.ServiceSites[(x.LocationName, x.ActivityType)];

                if (site.RequiredSkill == null)
                    return [];

                var required = site.RequiredSkill;

                var techSkill = tech.ServiceSkills
                    .FirstOrDefault(s => s.Type == required.Type);

                if (techSkill == null)
                {
                    var error = ValidationResultFactory.Hard($"[Skill Missing] {x.TechnicianName} has no skill for {required.Type} at {x.LocationName}");
                    x.ValidationResult.Add(error);
                    return
                    [
                        error
                    ];
                }

                if (techSkill.Level < required.Level)
                {
                    var error = ValidationResultFactory.Soft($"[Skill Level] {x.TechnicianName} has {techSkill.Level}, requires {required.Level} for {required.Type} at {x.LocationName}");
                    x.ValidationResult.Add(error);
                    return new[]
                    {
                        error
                    };
                }

                return [];
            })];
    }
}