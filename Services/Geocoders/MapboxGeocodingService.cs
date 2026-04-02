
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoChecker.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AutoChecker.Interfaces;
using System.Text.Json.Serialization;

namespace AutoChecker.Services.Geocoders;

public class MapboxGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly string? _accessToken;
    private readonly ILogger<MapboxGeocodingService> _logger;

    public MapboxGeocodingService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MapboxGeocodingService> logger)
    {
        _httpClient = httpClient;
        _accessToken = configuration["Mapbox:AccessToken"];
        _logger = logger;
        
        if (string.IsNullOrEmpty(_accessToken))
        {
            _logger.LogWarning("Mapbox access token is not configured");
        }
    }

    public async Task<(double Latitude, double Longitude)?> GeocodeAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address) || string.IsNullOrEmpty(_accessToken))
            return null;

        try
        {
            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"https://api.mapbox.com/geocoding/v5/mapbox.places/{encodedAddress}.json?" +
                      $"access_token={_accessToken}&permanent=true&limit=1";
            
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            
            var result = JsonSerializer.Deserialize<MapboxGeocodeResponse>(json);
            
            if (result?.Features != null && result.Features.Any())
            {
                var feature = result.Features.First();
                if (feature.Center != null && feature.Center.Length >= 2)
                {
                    return (feature.Center[1], feature.Center[0]);
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error geocoding address: {address}");
            return null;
        }
    }

    public async Task GeocodeAddressesAsync(
        IEnumerable<string> addresses,
        Action<string, (double Lat, double Lon)?> onGeocoded)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            _logger.LogWarning("Mapbox access token missing, skipping geocoding");
            return;
        }

        var uniqueAddresses = addresses
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct()
            .ToList();

        if (uniqueAddresses.Count == 0)
            return;

        _logger.LogInformation($"Mapbox: Geocoding {uniqueAddresses.Count} unique addresses");

        for (int i = 0; i < uniqueAddresses.Count; i++)
        {
            var address = uniqueAddresses[i];
            var result = await GeocodeAsync(address);
            onGeocoded(address, result);

            if ((i + 1) % 10 == 0)
            {
                _logger.LogInformation($"Mapbox geocoding progress: {i + 1}/{uniqueAddresses.Count}");
            }
        }

        _logger.LogInformation($"Mapbox: Completed geocoding {uniqueAddresses.Count} addresses");
    }

    private class MapboxGeocodeResponse
    {
        [JsonPropertyName("features")]
        public List<MapboxFeature> Features { get; set; } = new();
    }

    private class MapboxFeature
    {
        [JsonPropertyName("center")]
        public double[] Center { get; set; } = new double[2];
        
        [JsonPropertyName("place_name")]
        public string PlaceName { get; set; } = "";
    }
}