using AutoChecker.Models.Enums;

namespace AutoChecker.Models;

public class ServiceSite
{
    public string Name { get; set; } = null!;
    public ActivityType Service { get; set; } 

    public string Address { get; set; } = null!;
    public string CurrentTechnician { get; set; } = null!;

    public int TechsNeeded { get; set; }

    public AccessType Access { get; set; }

    public bool PermitRequired { get; set; }
    public PermitDifficulty? PermitDifficulty { get; set; }
    public List<string> TechsWithPermit { get; set; } = [];

    public Dictionary<DayOfWeek, (TimeSpan From, TimeSpan To)> WorkingHours { get; set; } = [];

    public VisitFrequency Frequency { get; set; } = null!;
    public int DurationMinutes { get; set; }

    public ServiceSkill? RequiredSkill { get; set; }

    public bool PhysicallyDemanding { get; set; }
    public bool HasLivingWalls { get; set; }
    public bool WorkAtHeights { get; set; }
    public bool RequiresLift { get; set; }
    public bool RequiresPesticides { get; set; }
    public bool RequiresCitizen { get; set; }

    public List<string> PreferredTechnicians { get; set; } = [];
    public List<string> ExcludedTechnicians { get; set; } = [];
}
