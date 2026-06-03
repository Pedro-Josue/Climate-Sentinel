using ClimateSentinel.DTOs;

namespace ClimateSentinel.Interfaces;

public interface IClimateProvider
{
    Task<OpenMeteoResponseDto> ObterDadosAsync(double latitude, double longitude);
}
