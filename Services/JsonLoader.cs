using AutoChecker.Models;
using System.Text.Json;

namespace AutoChecker.Services;

public static class JsonLoader
{
    public static List<ScheduleItem> Load(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<ScheduleItem>>(json) ?? new List<ScheduleItem>();
    }
}
