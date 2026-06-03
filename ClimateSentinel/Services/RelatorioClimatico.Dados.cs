using ClimateSentinel.Models;

namespace ClimateSentinel.Services;

public partial class RelatorioClimatico
{
    public RelatorioClimatico(
        IReadOnlyList<MonitoramentoRegistro> consultas,
        IReadOnlyList<AlertaClimatico> alertas)
    {
        Consultas = consultas;
        Alertas = alertas;
    }

    public IReadOnlyList<MonitoramentoRegistro> Consultas { get; }

    public IReadOnlyList<AlertaClimatico> Alertas { get; }

    public double MediaTemperatura => Consultas.Count == 0
        ? 0
        : Consultas.Average(item => item.DadoClimatico.Temperatura);

    public double MediaPrecipitacao => Consultas.Count == 0
        ? 0
        : Consultas.Average(item => item.DadoClimatico.Precipitacao);

    public int TotalConsultas => Consultas.Count;

    public int TotalAlertas => Alertas.Count;

    public IReadOnlyList<string> EventosDetectados => Consultas
        .SelectMany(item => item.Eventos)
        .Select(item => item.Nome)
        .Distinct()
        .ToList();
}
