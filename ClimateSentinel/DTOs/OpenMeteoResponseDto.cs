using System.Text.Json.Serialization;

namespace ClimateSentinel.DTOs;

public sealed class OpenMeteoResponseDto
{
    [JsonPropertyName("current")]
    public OpenMeteoCurrentDto? Current { get; set; }
}

public sealed class OpenMeteoCurrentDto
{
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    [JsonPropertyName("temperature_2m")]
    public double Temperature2M { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public double RelativeHumidity2M { get; set; }

    [JsonPropertyName("precipitation")]
    public double Precipitation { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed10M { get; set; }
}
