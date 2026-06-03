namespace ClimateSentinel.Models;

public sealed class Tempestade : EventoExtremo
{
    public Tempestade() : base(nameof(Tempestade), "Crítico")
    {
    }

    public override string GerarDescricao() => "Ventos intensos com risco de tempestades e danos estruturais.";
}
