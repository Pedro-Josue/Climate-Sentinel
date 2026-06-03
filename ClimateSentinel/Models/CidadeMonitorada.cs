namespace ClimateSentinel.Models;

public sealed class CidadeMonitorada
{
    public CidadeMonitorada(int id, string nome, string estado, CoordenadaGeografica coordenadas)
    {
        Id = id;
        Nome = nome;
        Estado = estado;
        Coordenadas = coordenadas;
    }

    public int Id { get; }

    public string Nome { get; }

    public string Estado { get; }

    public CoordenadaGeografica Coordenadas { get; }

    public DadoClimatico? DadoClimatico { get; set; }

    public override string ToString() => $"{Nome} - {Estado}";
}
