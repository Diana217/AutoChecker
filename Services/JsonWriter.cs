using AutoChecker.Models;
using AutoChecker.Models.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoChecker.Services;

public static class JsonWriter
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    
    public static void SerializeTechnicians(Dictionary<string, Technician> technicians, string filePath)
    {
        EnsureDirectoryExists(filePath);
        var json = JsonSerializer.Serialize(technicians, _jsonOptions);
        File.WriteAllText(filePath, json);
    }

    
    public static void SerializeServiceSites(
        Dictionary<(string Name, ActivityType Service), ServiceSite> serviceSites, 
        string filePath)
    {
        EnsureDirectoryExists(filePath);
        
        var serializableDict = serviceSites.ToDictionary(
            kvp => $"{kvp.Key.Name},{kvp.Key.Service}",
            kvp => kvp.Value
        );
        
        var json = JsonSerializer.Serialize(serializableDict, _jsonOptions);
        File.WriteAllText(filePath, json);
    }

    public static void SerializeVisits(List<ScheduleItem> visits, string filePath)
    {
        EnsureDirectoryExists(filePath);
        var json = JsonSerializer.Serialize(visits, _jsonOptions);
        File.WriteAllText(filePath, json);
    }

    public static void SerializeGeocodingCache(
        Dictionary<string, (double Lat, double Lon)> cache, 
        string path)
    {
        var dict = cache.ToDictionary(
            kvp => kvp.Key,
            kvp => new GeocodeResult
            {
                Address = kvp.Key,
                Latitude = kvp.Value.Lat,
                Longitude = kvp.Value.Lon
            }
        );
        
        var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
        
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        
        File.WriteAllText(path, json);
    }


    private static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}