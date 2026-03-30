// Services/NominatimGeocodingService.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoChecker.Interfaces;
using System.Text.Json.Serialization;

namespace AutoChecker.Services.Geocoders;

public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NominatimGeocodingService> _logger;
    private readonly SemaphoreSlim _rateLimiter;
    private const int REQUEST_DELAY_MS = 1100;

    public NominatimGeocodingService(
        HttpClient httpClient,
        ILogger<NominatimGeocodingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _rateLimiter = new SemaphoreSlim(1, 1);

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AutoChecker/1.0");
    }

    public async Task<(double Latitude, double Longitude)?> GeocodeAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return null;

        try
        {
            await _rateLimiter.WaitAsync();
            await Task.Delay(REQUEST_DELAY_MS);

            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"https://nominatim.openstreetmap.org/search?q={encodedAddress}&format=json&limit=1";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Nominatim returned {response.StatusCode} for address: {address}");
                return null;
            }

            var results = JsonSerializer.Deserialize<List<NominatimResult>>(json);
            // Console.WriteLine($"Nominatim response for '{address}': {json}");

            if (results != null && results.Any())
            {
                var result = results.First();

                if (double.TryParse(result.lat, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) &&
                    double.TryParse(result.lon, NumberStyles.Any, CultureInfo.InvariantCulture, out var lon))
                {
                    _logger.LogDebug($"Geocoded: '{address}' -> ({lat}, {lon})");
                    return (lat, lon);
                }
            }

            _logger.LogDebug($"No results for address: {address}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error geocoding address: {address}");
            return null;
        }
        finally
        {
            _rateLimiter.Release();
        }
    }

    public async Task GeocodeAddressesAsync(
        IEnumerable<string> addresses,
        Action<string, (double Lat, double Lon)?> onGeocoded)
    {
        var uniqueAddresses = addresses
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct()
            .ToList();

        _logger.LogInformation($"Geocoding {uniqueAddresses.Count} unique addresses");

        for (int i = 0; i < uniqueAddresses.Count; i++)
        {
            var address = uniqueAddresses[i];
            var result = await GeocodeAsync(address);
            onGeocoded(address, result);

            if ((i + 1) % 10 == 0)
            {
                _logger.LogInformation($"Geocoding progress: {i + 1}/{uniqueAddresses.Count}");
            }
        }

        _logger.LogInformation($"Completed geocoding {uniqueAddresses.Count} addresses");
    }


    private class NominatimResult
    {
        [JsonPropertyName("lat")]
        public string lat { get; set; } = "";

        [JsonPropertyName("lon")]
        public string lon { get; set; } = "";

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = "";
    }
}