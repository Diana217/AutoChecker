// Services/MapboxRouteChecker.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoChecker.Interfaces;
using AutoChecker.Models;
using AutoChecker.Models.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoChecker.Services.RouteCheckers;
public class MapboxRouteChecker : IRouteChecker
{
    private readonly HttpClient _httpClient;
    private readonly string? _accessToken;
    private readonly ILogger<MapboxRouteChecker> _logger;
    private readonly Dictionary<(double Lat1, double Lon1, double Lat2, double Lon2), (double Distance, double Duration)> _cache;

    public MapboxRouteChecker(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MapboxRouteChecker> logger)
    {
        _httpClient = httpClient;
        _accessToken = configuration["Mapbox:AccessToken"];
        _logger = logger;
        _cache = new Dictionary<(double, double, double, double), (double, double)>();
        
        if (string.IsNullOrEmpty(_accessToken))
        {
            _logger.LogWarning("Mapbox access token is not configured");
        }
    }

    public async Task CheckRoutesAsync(ValidationContext context)
    {
        foreach (var visit in context.Visits)
        {
            if (visit.ActivityType == ActivityType.Commute)
            {
                await CheckCommuteAsync(visit);
            }
        }
    }

    private async Task CheckCommuteAsync(ScheduleItem visit)
    {
        var err = false;
        if (!visit.LatitudeName.HasValue || !visit.LongitudeName.HasValue)
        {
            err = true;            
            Console.WriteLine($"[Missing Coordinates] No coordinates for start location: {visit.LocationName}");
        }

        if (!visit.LatitudeTo.HasValue || !visit.LongitudeTo.HasValue)
        {
            err = true;
            Console.WriteLine($"[Missing Coordinates] No coordinates for destination: {visit.LocationTo}");
        }

        if (err)
            return;

        var route = await GetRouteAsync(
            visit.LongitudeName.Value, visit.LatitudeName.Value,
            visit.LongitudeTo.Value, visit.LatitudeTo.Value);

        if (!route.HasValue)
        {
            Console.WriteLine($"[Route Error] Could not calculate route from {visit.LocationName} to {visit.LocationTo}");
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
                var error = ValidationResultFactory.Hard(
                    $"[Commute Time] Route for {visit.TechnicianName} from {visit.LocationName} to {visit.LocationTo} " +
                    $"takes {(visit.RouteDuration ?? TimeSpan.Zero).TotalMinutes:F0} min, " +
                    $"but planned {plannedDuration.TotalMinutes:F0} min (exceeds by {difference.TotalMinutes:F0} min)");

                Console.WriteLine($"HARD:{error.Message}");
                visit.ValidationResult.Add(error);
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
            var url = $"https://api.mapbox.com/directions/v5/mapbox/driving/" +
                      $"{lon1.ToString(culture)},{lat1.ToString(culture)};" +
                      $"{lon2.ToString(culture)},{lat2.ToString(culture)}?" +
                      $"access_token={_accessToken}&overview=false&steps=false";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Mapbox Directions returned {response.StatusCode} for route: {lon1},{lat1} -> {lon2},{lat2}");
                return null;
            }

            var result = JsonSerializer.Deserialize<MapboxDirectionsResponse>(json);

            if (result?.Code == "Ok" && result.Routes != null && result.Routes.Any())
            {
                var route = result.Routes.First();
                var data = (route.Distance, route.Duration);

                lock (_cache)
                {
                    _cache[cacheKey] = data;
                }

                _logger.LogInformation($"Mapbox route: {route.Distance}m, duration: {route.Duration}s");
                return data;
            }

            _logger.LogWarning($"Mapbox returned code: {result?.Code} for route: {lon1},{lat1} -> {lon2},{lat2}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Mapbox request failed for route: {lon1},{lat1} -> {lon2},{lat2}");
            return null;
        }
    }

    private class MapboxDirectionsResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("routes")]
        public List<MapboxRoute> Routes { get; set; } = new();
    }

    private class MapboxRoute
    {
        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }
    }
}