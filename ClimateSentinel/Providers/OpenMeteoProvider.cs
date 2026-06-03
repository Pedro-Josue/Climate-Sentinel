using System.Net;
using System.Text.Json;
using ClimateSentinel.DTOs;
using ClimateSentinel.Exceptions;
using ClimateSentinel.Interfaces;

namespace ClimateSentinel.Providers;

public sealed class OpenMeteoProvider : IClimateProvider, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private bool _disposed;

    public OpenMeteoProvider(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient
        {
            BaseAddress = new Uri("https://api.open-meteo.com/")
        };
    }

    public async Task<OpenMeteoResponseDto> ObterDadosAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"v1/forecast?latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      "&current=temperature_2m,relative_humidity_2m,precipitation,wind_speed_10m" +
                      "&timezone=America%2FSao_Paulo";

            using var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiCommunicationException(
                    $"A API Open-Meteo retornou o código {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<OpenMeteoResponseDto>(content, JsonOptions);

            return result ?? throw new ApiCommunicationException("A resposta da API veio vazia ou inválida.");
        }
        catch (TaskCanceledException ex)
        {
            throw new ApiCommunicationException("A consulta à API demorou demais para responder.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiCommunicationException("Não foi possível comunicar com a API Open-Meteo.", ex);
        }
        catch (JsonException ex)
        {
            throw new ApiCommunicationException("Falha ao interpretar os dados retornados pela API.", ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _httpClient.Dispose();
        _disposed = true;
    }
}
