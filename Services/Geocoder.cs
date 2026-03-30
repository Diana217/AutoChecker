using AutoChecker.Models;
using System.Text.Json;
using AutoChecker.Interfaces;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services;

public static class Geocoder
{
    public static async Task GeocodeAsync(ValidationContext context, IGeocodingService geocodingService)
    {
        var basePath = AppContext.BaseDirectory;
        var cachePath = Path.Combine(basePath, "Data", "Cache", "geocoding_cache.json");
        var cache = JsonLoader.LoadGeocodingCache(cachePath);

        var allAddresses = new HashSet<string>();
        
        foreach (var visit in context.Visits)
        {
            var addressName = ResolveAddress(visit.LocationName, visit.TechnicianName, visit.LocationName, context);
            if (!string.IsNullOrEmpty(addressName))
                allAddresses.Add(addressName);

            if (!string.IsNullOrEmpty(visit.LocationTo))
            {
                var addressTo = ResolveAddress(visit.LocationTo, visit.TechnicianName, visit.LocationTo, context);
                if (!string.IsNullOrEmpty(addressTo))
                    allAddresses.Add(addressTo);
            }
        }

        var uncachedAddresses = allAddresses
            .Where(addr => !cache.ContainsKey(NormalizeAddress(addr)))
            .ToList();

        if (uncachedAddresses.Any())
        {            
            
            await geocodingService.GeocodeAddressesAsync(uncachedAddresses, (address, coords) =>
            {
                if (coords.HasValue)
                {
                    cache[NormalizeAddress(address)] = coords.Value;
                }
            });
            
            JsonWriter.SerializeGeocodingCache(cache, cachePath);
        }

        foreach (var visit in context.Visits)
        {
            var addressName = ResolveAddress(visit.LocationName, visit.TechnicianName, visit.LocationName, context);
            if (!string.IsNullOrEmpty(addressName) && cache.TryGetValue(NormalizeAddress(addressName), out var coordsName))
            {
                visit.LatitudeName = coordsName.Lat;
                visit.LongitudeName = coordsName.Lon;
            }

            if (!string.IsNullOrEmpty(visit.LocationTo))
            {
                var addressTo = ResolveAddress(visit.LocationTo, visit.TechnicianName, visit.LocationTo, context);
                if (!string.IsNullOrEmpty(addressTo) && cache.TryGetValue(NormalizeAddress(addressTo), out var coordsTo))
                {
                    visit.LatitudeTo = coordsTo.Lat;
                    visit.LongitudeTo = coordsTo.Lon;
                }
            }
        }
    }

    private static string ResolveAddress(string locationName, string technicianName, string serviceName, ValidationContext context)
    {
        if (Matches(locationName, LocationType.Home))
            return context.Technicians[technicianName].HomeAddress;

        if (Matches(locationName, LocationType.Office))
            return context.Technicians[technicianName].OfficeAddress;

        return context.ServiceSites.Where(x => x.Key.Name == locationName).Select(x => x.Value.Address).FirstOrDefault() ?? "";
    }

    private static bool Matches(string location, LocationType type) =>
        string.Equals(location, type.ToString(), StringComparison.OrdinalIgnoreCase);

    private static string NormalizeAddress(string address) =>
        address.Trim().ToLowerInvariant().Replace(" ", "");
}