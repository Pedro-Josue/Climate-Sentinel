namespace ClimateSentinel.Models;

public abstract class EventoExtremo
{
    protected EventoExtremo(string nome, string nivelRisco)
    {
        Nome = nome;
        NivelRisco = nivelRisco;
    }

    public string Nome { get; }

    public string NivelRisco { get; }

    public abstract string GerarDescricao();
}
