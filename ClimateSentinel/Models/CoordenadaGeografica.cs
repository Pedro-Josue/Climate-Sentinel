namespace ClimateSentinel.Models;

public readonly struct CoordenadaGeografica(double latitude, double longitude)
{
    public double Latitude { get; } = latitude;

    public double Longitude { get; } = longitude;
}
