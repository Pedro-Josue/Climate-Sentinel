namespace ClimateSentinel.Models;

public sealed class OndaDeCalor : EventoExtremo
{
    public OndaDeCalor() : base(nameof(OndaDeCalor), "Crítico")
    {
    }

    public override string GerarDescricao() => "Temperatura extrema com potencial de impacto na saúde da população.";
}
