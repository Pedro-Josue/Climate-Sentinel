namespace ClimateSentinel.Models;

public sealed class Enchente : EventoExtremo
{
    public Enchente() : base(nameof(Enchente), "Crítico")
    {
    }

    public override string GerarDescricao() => "Precipitação elevada com risco de enchentes e alagamentos.";
}
