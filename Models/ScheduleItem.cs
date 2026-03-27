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
}
