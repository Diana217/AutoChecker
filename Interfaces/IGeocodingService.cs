using AutoChecker.Models;
using System.Threading.Tasks;

namespace AutoChecker.Interfaces;

public interface IGeocodingService
{
    Task<(double Latitude, double Longitude)?> GeocodeAsync(string address);
    Task GeocodeAddressesAsync(IEnumerable<string> addresses, Action<string, (double Lat, double Lon)?> onGeocoded);
}