namespace ClimateSentinel.Models;

public sealed class DadoClimatico
{
    public double Temperatura { get; set; }

    public double Umidade { get; set; }

    public double Precipitacao { get; set; }

    public double VelocidadeVento { get; set; }

    public DateTime DataHora { get; set; }
}
