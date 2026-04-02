using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoChecker.Models;
using AutoChecker.Models.Enums;
using Microsoft.Extensions.Logging;
using AutoChecker.Interfaces;
using System.Text.Json.Serialization;

namespace AutoChecker.Services.RouteCheckers;

public class OsrmRouteChecker : IRouteChecker
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OsrmRouteChecker> _logger;
    private readonly Dictionary<(double Lat1, double Lon1, double Lat2, double Lon2), (double Distance, double Duration)> _cache;

    public OsrmRouteChecker(
        HttpClient httpClient,
        ILogger<OsrmRouteChecker> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = new Dictionary<(double, double, double, double), (double, double)>();
    }

    public async Task CheckRoutesAsync(ValidationContext context)
    {
        foreach (var visit in context.Visits)
        {
            if (visit.ActivityType == ActivityType.Commute)
            {
                await CheckCommuteAsync(visit, context);
                
            }
            
        }
    }

    private async Task CheckCommuteAsync(ScheduleItem visit, ValidationContext context)
    {
        var err = false;
        if (!visit.LatitudeName.HasValue || !visit.LongitudeName.HasValue)
        {
            err = true;            
            // Console.WriteLine($"[Missing Coordinates] No coordinates for start location: {visit.LocationName}");
           
        }

        if (!visit.LatitudeTo.HasValue || !visit.LongitudeTo.HasValue)
        {
            err = true;            
            // Console.WriteLine($"[Missing Coordinates] No coordinates for destination: {visit.LocationTo}");
            
        }

        if(err)
            return;

        
        var route = await GetRouteAsync(
            visit.LongitudeName.Value, visit.LatitudeName.Value,
            visit.LongitudeTo.Value, visit.LatitudeTo.Value);
        
        if (!route.HasValue)
        {
            // Console.WriteLine($"[Route Error] Could not calculate route from {visit.LocationName} to {visit.LocationTo}");
            return;
        }

        
        visit.RouteDuration = TimeSpan.FromSeconds(route.Value.Duration);
        visit.RouteDistance = route.Value.Distance;

        var plannedDuration = visit.EndTime - visit.StartTime;

        if (plannedDuration.TotalMinutes > 0)
        {
            var difference = (visit.RouteDuration ?? TimeSpan.Zero) - plannedDuration;
            
            if (difference.TotalMinutes > 0)
            {
                if (difference.TotalMinutes > 0.1 * (visit.RouteDuration ?? TimeSpan.Zero).TotalMinutes)
                {
                    var error = ValidationResultFactory.Hard(
                        $"[Commute Time] Route for {visit.TechnicianName} from {visit.LocationName} to {visit.LocationTo} takes {(visit.RouteDuration ?? TimeSpan.Zero).TotalMinutes:F0} min, " +
                        $"but planned {plannedDuration.TotalMinutes:F0} min (exceeds by {difference.TotalMinutes:F0} min)");
                    visit.ValidationResults.Add(error);
                }
                else
                {
                    var error = ValidationResultFactory.Soft(
                        $"[Commute Time Warning] Route for {visit.TechnicianName} from {visit.LocationName} to {visit.LocationTo} takes {(visit.RouteDuration ?? TimeSpan.Zero).TotalMinutes:F0} min, " +
                        $"but planned {plannedDuration.TotalMinutes:F0} min (exceeds by {difference.TotalMinutes:F0} min (within 10%))");
                    visit.ValidationResults.Add(error);
                }
            }
        }
    }


    private async Task<(double Distance, double Duration)?> GetRouteAsync(double lon1, double lat1, double lon2, double lat2)
    {
        var cacheKey = (lat1, lon1, lat2, lon2);
        
        lock (_cache)
        {
            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;
        }
    
        try
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            
            var url = $"https://router.project-osrm.org/route/v1/driving/" +
                      $"{lon1.ToString(culture)},{lat1.ToString(culture)};" +
                      $"{lon2.ToString(culture)},{lat2.ToString(culture)}?overview=false";
            
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"OSRM returned {response.StatusCode} for route");
                return null;
            }
            
            var result = JsonSerializer.Deserialize<OsrmResponse>(json);
            
            if (result?.Code == "Ok" && result.Routes != null && result.Routes.Any())
            {
                var route = result.Routes.First();
                var data = (route.Distance, route.Duration);
                
                lock (_cache)
                {
                    _cache[cacheKey] = data;
                }
                
                _logger.LogInformation($"Route distance: {route.Distance}m, duration: {route.Duration}s");
                return data;
            }
            
            _logger.LogWarning($"OSRM returned code: {result?.Code}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"OSRM request failed for route: {lon1},{lat1} -> {lon2},{lat2}");
            return null;
        }
    }

    private class OsrmResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";
        
        [JsonPropertyName("routes")]
        public List<OsrmRoute> Routes { get; set; } = new();
    }
    
    private class OsrmRoute
    {
        [JsonPropertyName("distance")]
        public double Distance { get; set; }
        
        [JsonPropertyName("duration")]
        public double Duration { get; set; }
        
        [JsonPropertyName("legs")]
        public List<OsrmLeg> Legs { get; set; } = new();
    }
    
    private class OsrmLeg
    {
        [JsonPropertyName("distance")]
        public double Distance { get; set; }
        
        [JsonPropertyName("duration")]
        public double Duration { get; set; }
    }
}