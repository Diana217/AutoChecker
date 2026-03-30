using AutoChecker.Models.Enums;

namespace AutoChecker.Models;

public class Technician
{
    public string Name { get; set; } = null!;
    public string HomeAddress { get; set; } = null!;
    public string OfficeAddress { get; set; } = null!;

    public LocationType StartsFrom { get; set; }
    public LocationType FinishesAt { get; set; }

    public Dictionary<DayOfWeek, (TimeSpan From, TimeSpan To)> WorkingHours { get; set; } = [];

    public int MinBreakMinutes { get; set; }
    public TimeSpan BreakNotEarlierThan { get; set; }
    public TimeSpan BreakNotLaterThan { get; set; }

    public int MaxHoursPerDay { get; set; }
    public int MaxHoursPerWeek { get; set; }

    public List<ServiceSkill> ServiceSkills { get; set; } = [];

    public bool CanDoPhysicalWork { get; set; }
    public bool SkilledInLivingWalls { get; set; }
    public bool ComfortableWithHeights { get; set; }
    public bool CertifiedLift { get; set; }
    public bool PesticideCertification { get; set; }
    public bool IsCitizen { get; set; }

    public List<ValidationResult> ValidationResults { get; set; } = [];
}
