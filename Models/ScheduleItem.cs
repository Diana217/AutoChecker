using AutoChecker.Models.Enums;
using System.Text.Json.Serialization;

namespace AutoChecker.Models;

public class ScheduleItem
{
    [JsonPropertyName("date")]
    public string DateString { get; set; } = "";

    [JsonPropertyName("start_time")]
    public string StartTimeString { get; set; } = "";

    [JsonPropertyName("end_time")]
    public string EndTimeString { get; set; } = "";

    [JsonPropertyName("technician_name")]
    public string TechnicianName { get; set; } = "";

    [JsonPropertyName("location_name")]
    public string LocationName { get; set; } = "";

    [JsonPropertyName("location_to")]
    public string? LocationTo { get; set; }

    [JsonPropertyName("activity_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ActivityType ActivityType { get; set; }

    [JsonIgnore]
    public DateTime Date => DateTime.Parse(DateString);

    [JsonIgnore]
    public TimeSpan StartTime => TimeSpan.Parse(StartTimeString);

    [JsonIgnore]
    public TimeSpan EndTime => TimeSpan.Parse(EndTimeString);

    [JsonPropertyName("latitude_to")]
    public double? LatitudeTo { get; set; }
    [JsonPropertyName("longitude_to")]
    public double? LongitudeTo { get; set; }

    [JsonPropertyName("latitude_name")]
    public double? LatitudeName { get; set; }
    [JsonPropertyName("longitude_name")]
    public double? LongitudeName { get; set; }

    [JsonPropertyName("route_duration")]
    public TimeSpan? RouteDuration { get; set; }

    [JsonPropertyName("route_distance")]
    public double? RouteDistance { get; set; }

    
    public List<ValidationResult> ValidationResults { get; set; } = [];
}
