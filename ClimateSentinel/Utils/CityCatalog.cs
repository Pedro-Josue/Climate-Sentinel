using ClimateSentinel.Models;

namespace ClimateSentinel.Utils;

public static class CityCatalog
{
    public static IReadOnlyList<CidadeMonitorada> ObterCidades() => new[]
    {
        new CidadeMonitorada(1, "São Paulo", "SP", new CoordenadaGeografica(-23.5505, -46.6333)),
        new CidadeMonitorada(2, "Rio de Janeiro", "RJ", new CoordenadaGeografica(-22.9068, -43.1729)),
        new CidadeMonitorada(3, "Recife", "PE", new CoordenadaGeografica(-8.0476, -34.8770)),
        new CidadeMonitorada(4, "Porto Alegre", "RS", new CoordenadaGeografica(-30.0346, -51.2177)),
        new CidadeMonitorada(5, "Manaus", "AM", new CoordenadaGeografica(-3.1190, -60.0217))
    };
}
