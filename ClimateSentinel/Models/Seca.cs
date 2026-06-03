namespace ClimateSentinel.Models;

public sealed class Seca : EventoExtremo
{
    public Seca() : base(nameof(Seca), "Alto")
    {
    }

    public override string GerarDescricao() => "Umidade muito baixa indicando risco de seca e estiagem.";
}
