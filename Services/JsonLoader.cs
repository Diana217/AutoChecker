using AutoChecker.Models;
using System.Text.Json;

namespace AutoChecker.Services;

public static class JsonLoader
{
    public static List<ScheduleItem> LoadVisits(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<ScheduleItem>>(json) ?? new List<ScheduleItem>();
    }

    public static Dictionary<string, (double Lat, double Lon)> LoadGeocodingCache(string path)
    {
        if (!File.Exists(path))
            return new Dictionary<string, (double Lat, double Lon)>();
    
        var json = File.ReadAllText(path);
        var cache = JsonSerializer.Deserialize<Dictionary<string, GeocodeResult>>(json) 
                    ?? new Dictionary<string, GeocodeResult>();
        
        return cache.ToDictionary(
            kvp => kvp.Key, 
            kvp => (kvp.Value.Latitude, kvp.Value.Longitude)
        );
    }

}
