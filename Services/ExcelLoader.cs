using AutoChecker.Models;
using AutoChecker.Models.Enums;
using OfficeOpenXml;
using System.Globalization;

namespace AutoChecker.Services;

public static class ExcelLoader
{
    public static List<Technician> LoadTechnicians(string path)
    {
        ExcelPackage.License.SetNonCommercialPersonal("AutoChecker");

        using var package = new ExcelPackage(new FileInfo(path));
        var sheet = package.Workbook.Worksheets["Technicians"];

        var list = new List<Technician>();

        int row = 4;

        while (sheet.Cells[row, 1].Value != null)
        {
            var tech = new Technician
            {
                Name = sheet.Cells[$"A{row}"].Text,
                HomeAddress = sheet.Cells[$"B{row}"].Text,
                OfficeAddress = sheet.Cells[$"C{row}"].Text,
                StartsFrom = ParseLocation(sheet.Cells[$"D{row}"].Text),
                FinishesAt = ParseLocation(sheet.Cells[$"E{row}"].Text),

                WorkingHours = new Dictionary<DayOfWeek, (TimeSpan, TimeSpan)>
                {
                    { DayOfWeek.Monday, ParseTimeRange(sheet, row, "F", "G") },
                    { DayOfWeek.Tuesday, ParseTimeRange(sheet, row, "H", "I") },
                    { DayOfWeek.Wednesday, ParseTimeRange(sheet, row, "J", "K") },
                    { DayOfWeek.Thursday, ParseTimeRange(sheet, row, "L", "M") },
                    { DayOfWeek.Friday, ParseTimeRange(sheet, row, "N", "O") },
                    //{ DayOfWeek.Saturday, ParseTimeRange(sheet, row, "P", "Q") },
                    //{ DayOfWeek.Sunday, ParseTimeRange(sheet, row, "R", "S") }
                },

                MinBreakMinutes = ParseInt(sheet.Cells[$"T{row}"].Text),
                BreakNotEarlierThan = ParseTime(sheet.Cells[$"U{row}"].Text),
                BreakNotLaterThan = ParseTime(sheet.Cells[$"V{row}"].Text),

                MaxHoursPerDay = ParseInt(sheet.Cells[$"W{row}"].Text),
                MaxHoursPerWeek = ParseInt(sheet.Cells[$"X{row}"].Text),

                ServiceSkills = ParseServiceSkills(sheet.Cells[$"Y{row}"].Text),

                CanDoPhysicalWork = ParseBool(sheet.Cells[$"Z{row}"].Text),
                SkilledInLivingWalls = ParseBool(sheet.Cells[$"AA{row}"].Text),
                ComfortableWithHeights = ParseBool(sheet.Cells[$"AB{row}"].Text),
                CertifiedLift = ParseBool(sheet.Cells[$"AC{row}"].Text),
                PesticideCertification = ParseBool(sheet.Cells[$"AD{row}"].Text),
                IsCitizen = ParseBool(sheet.Cells[$"AE{row}"].Text)
            };

            list.Add(tech);
            row++;
        }

        return list;
    }

    public static List<ServiceSite> LoadServiceSites(string path)
    {
        ExcelPackage.License.SetNonCommercialPersonal("AutoChecker");

        using var package = new ExcelPackage(new FileInfo(path));
        var sheet = package.Workbook.Worksheets["Service sites"];

        var list = new List<ServiceSite>();

        int row = 4;

        while (sheet.Cells[row, 1].Value != null)
        {
            var site = new ServiceSite
            {
                Name = sheet.Cells[$"A{row}"].Text,
                Service = ParseServiceType(sheet.Cells[$"B{row}"].Text),
                Address = sheet.Cells[$"C{row}"].Text,
                CurrentTechnician = sheet.Cells[$"D{row}"].Text,

                TechsNeeded = ParseInt(sheet.Cells[$"E{row}"].Text),

                Access = ParseAccess(sheet.Cells[$"F{row}"].Text),

                PermitRequired = ParseBool(sheet.Cells[$"G{row}"].Text),
                PermitDifficulty = ParsePermitDifficulty(sheet.Cells[$"H{row}"].Text), 
                TechsWithPermit = ParseList(sheet.Cells[$"I{row}"].Text),

                WorkingHours = new Dictionary<DayOfWeek, (TimeSpan, TimeSpan)>
                {
                    { DayOfWeek.Monday, ParseTimeRange(sheet, row, "J", "K") },
                    { DayOfWeek.Tuesday, ParseTimeRange(sheet, row, "L", "M") },
                    { DayOfWeek.Wednesday, ParseTimeRange(sheet, row, "N", "O") },
                    { DayOfWeek.Thursday, ParseTimeRange(sheet, row, "P", "Q") },
                    { DayOfWeek.Friday, ParseTimeRange(sheet, row, "R", "S") },
                    //{ DayOfWeek.Saturday, ParseTimeRange(sheet, row, "T", "U") },
                    //{ DayOfWeek.Sunday, ParseTimeRange(sheet, row, "V", "W") }
                },

                Frequency = ParseFrequency(sheet.Cells[$"X{row}"].Text),
                DurationMinutes = ParseInt(sheet.Cells[$"Y{row}"].Text),

                RequiredSkill = ParseSkill(sheet.Cells[$"Z{row}"].Text),

                PhysicallyDemanding = ParseBool(sheet.Cells[$"AA{row}"].Text),
                HasLivingWalls = ParseBool(sheet.Cells[$"AB{row}"].Text),
                WorkAtHeights = ParseBool(sheet.Cells[$"AC{row}"].Text),
                RequiresLift = ParseBool(sheet.Cells[$"AD{row}"].Text),
                RequiresPesticides = ParseBool(sheet.Cells[$"AE{row}"].Text),
                RequiresCitizen = ParseBool(sheet.Cells[$"AF{row}"].Text),

                PreferredTechnicians = ParseList(sheet.Cells[$"AG{row}"].Text),
                ExcludedTechnicians = ParseList(sheet.Cells[$"AH{row}"].Text)
            };

            if (!list.Any(s => s.Name == site.Name && s.Service == site.Service))
            {
                list.Add(site);
            }

            row++;
        }

        return list;
    }

    private static (TimeSpan From, TimeSpan To) ParseTimeRange(ExcelWorksheet sheet, int row, string fromCol, string toCol)
    {
        return (
            ParseTime(sheet.Cells[$"{fromCol}{row}"].Text),
            ParseTime(sheet.Cells[$"{toCol}{row}"].Text)
        );
    }

    private static TimeSpan ParseTime(string value)
    {
        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result))
            return result;

        return TimeSpan.Zero;
    }

    private static int ParseInt(string value)
    {
        return int.TryParse(value, out var result) ? result : 0;
    }

    private static bool ParseBool(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.Trim().ToLower();

        return value == "yes";
    }

    private static LocationType ParseLocation(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return LocationType.Home;

        value = value.Trim().ToLower();

        return value switch
        {
            "home" => LocationType.Home,
            "office" => LocationType.Office,
            "either works" => LocationType.Either,
            _ => throw new Exception($"Unknown location type: {value}")
        };
    }

    private static List<ServiceSkill> ParseServiceSkills(string value)
    {
        var result = new List<ServiceSkill>();

        if (string.IsNullOrWhiteSpace(value))
            return result;

        var items = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var item in items)
        {
            var skill = ParseSkill(item);

            if (skill != null)
            {
                result.Add(skill);
            }
        }

        return result;
    }

    private static ServiceSkill? ParseSkill(string item)
    {
        var parts = item.Split('-', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return null;

        var skill = new ServiceSkill
        {
            Type = ParseServiceType(parts[0]),
            Level = ParseLevel(parts[1])
        };

        return skill;
    }

    private static ActivityType ParseServiceType(string value)
    {
        return value.Trim().ToLower() switch
        {
            "interior" => ActivityType.Interior,
            "exterior" => ActivityType.Exterior,
            "floral" => ActivityType.Floral,
            _ => throw new Exception($"Unknown service: {value}")
        };
    }

    private static SkillLevel ParseLevel(string value)
    {
        return value.Trim().ToLower() switch
        {
            "junior" => SkillLevel.Junior,
            "medior" => SkillLevel.Medior,
            "senior" => SkillLevel.Senior,
            _ => throw new Exception($"Unknown skill level: {value}")
        };
    }

    private static AccessType ParseAccess(string value)
    {
        value = value.ToLower();

        return value switch
        {
            "car/van" => AccessType.CarOrVan,
            "drive to a hub and then walk" => AccessType.HubAndWalk,
            "either works" => AccessType.Either,
            _ => throw new Exception($"Unknown access type: {value}")
        };
    }

    private static PermitDifficulty? ParsePermitDifficulty(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.ToLower() switch
        {
            "easy" => PermitDifficulty.Easy,
            "medium" => PermitDifficulty.Medium,
            "hard" => PermitDifficulty.Hard,
            _ => null
        };
    }

    private static VisitFrequency ParseFrequency(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new VisitFrequency();

        value = value.ToLower().Trim();

        if (value == "unknown")
            return new VisitFrequency { IsUnknown = true };

        if (value.Contains("week"))
        {
            var times = int.Parse(value[0].ToString());
            return new VisitFrequency { Times = times, DaysPeriod = 7 };
        }

        if (value.Contains("in"))
        {
            // 1x in 14 days
            var parts = value.Split(' ');
            return new VisitFrequency
            {
                Times = int.Parse(parts[0][0].ToString()),
                DaysPeriod = int.Parse(parts[2])
            };
        }

        throw new Exception($"Unknown frequency: {value}");
    }

    private static List<string> ParseList(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        return [.. value
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())];
    }
}
