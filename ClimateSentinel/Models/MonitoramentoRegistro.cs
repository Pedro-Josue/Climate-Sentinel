namespace ClimateSentinel.Models;

public sealed class MonitoramentoRegistro
{
    public CidadeMonitorada Cidade { get; init; } = default!;

    public DadoClimatico DadoClimatico { get; init; } = default!;

    public DateTime DataConsulta { get; init; }

    public IReadOnlyList<EventoExtremo> Eventos { get; init; } = Array.Empty<EventoExtremo>();
}
