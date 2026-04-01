using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services.Validators;

public class SkillValidator : IValidator
{
    public List<ValidationResult> Validate(ValidationContext context)
    {
        return [.. context.Visits
            .Where(x => x.ActivityType != ActivityType.Commute)
            .Where(x => context.Technicians.ContainsKey(x.TechnicianName))
            .Where(x => context.ServiceSites.ContainsKey((x.LocationName, x.ActivityType)))
            .Where(x =>
            {
                var tech = context.Technicians[x.TechnicianName];
                var site = context.ServiceSites[(x.LocationName, x.ActivityType)];

                return
                    (site.PhysicallyDemanding && !tech.CanDoPhysicalWork) ||
                    (site.HasLivingWalls && !tech.SkilledInLivingWalls) ||
                    (site.WorkAtHeights && !tech.ComfortableWithHeights) ||
                    (site.RequiresLift && !tech.CertifiedLift) ||
                    (site.RequiresPesticides && !tech.PesticideCertification) ||
                    (site.RequiresCitizen && !tech.IsCitizen);
            })
            .SelectMany(x =>
            {
                var site = context.ServiceSites[(x.LocationName, x.ActivityType)];
                var tech = context.Technicians[x.TechnicianName];

                var hardReasons = new List<string>();
                var softReasons = new List<string>();

                if (site.RequiresCitizen && !tech.IsCitizen)
                    hardReasons.Add("citizenship");

                if (site.PhysicallyDemanding && !tech.CanDoPhysicalWork)
                    softReasons.Add("physically demanding");

                if (site.HasLivingWalls && !tech.SkilledInLivingWalls)
                    softReasons.Add("living walls");

                if (site.WorkAtHeights && !tech.ComfortableWithHeights)
                    softReasons.Add("heights");

                if (site.RequiresLift && !tech.CertifiedLift)
                    softReasons.Add("lift");

                if (site.RequiresPesticides && !tech.PesticideCertification)
                    softReasons.Add("pesticides");

                var results = new List<ValidationResult>();

                if (hardReasons.Any())
                {
                    var error = ValidationResultFactory.Hard(
                        $"[Skills] {x.TechnicianName} cannot work at {x.LocationName} ({string.Join(", ", hardReasons)})"
                    );
                    x.ValidationResults.Add(error);
                    results.Add(error);
                }

                if (softReasons.Any())
                {
                    var error = ValidationResultFactory.Soft(
                        $"[Skills] {x.TechnicianName} suboptimal for {x.LocationName} ({string.Join(", ", softReasons)})"
                    );
                    x.ValidationResults.Add(error);
                    results.Add(error);
                }

                return results;
            })];
    }
}