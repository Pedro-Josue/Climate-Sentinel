namespace ClimateSentinel.Models;

public sealed class AlertaClimatico
{
    public string Severidade { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; }

    public string TipoEvento { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;
}
