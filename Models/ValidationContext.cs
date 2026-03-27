using AutoChecker.Models.Enums;

namespace AutoChecker.Models;

public class ValidationContext
{
    public Dictionary<string, Technician> Technicians { get; set; } = [];
    public Dictionary<(string Name, ActivityType Service), ServiceSite> ServiceSites { get; set; } = [];
    public List<ScheduleItem> Visits { get; set; } = [];
}
