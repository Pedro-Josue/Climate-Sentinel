using ClimateSentinel.Exceptions;
using ClimateSentinel.Models;

namespace ClimateSentinel.Services;

public sealed class ClimateAnalysisService
{
    public (IReadOnlyList<EventoExtremo> Eventos, IReadOnlyList<AlertaClimatico> Alertas)
        Analisar(DadoClimatico dados, CidadeMonitorada cidade)
    {
        try
        {
            var eventos = new List<EventoExtremo>();
            var alertas = new List<AlertaClimatico>();

            if (dados.Temperatura >= 40)
            {
                AdicionarEvento(new OndaDeCalor());
            }

            if (dados.Umidade <= 20)
            {
                AdicionarEvento(new Seca());
            }

            if (dados.Precipitacao >= 80)
            {
                AdicionarEvento(new Enchente());
            }

            if (dados.VelocidadeVento >= 70)
            {
                AdicionarEvento(new Tempestade());
            }

            return (eventos, alertas);

            void AdicionarEvento(EventoExtremo evento)
            {
                eventos.Add(evento);
                alertas.Add(new AlertaClimatico
                {
                    Severidade = evento.NivelRisco,
                    DataCriacao = DateTime.Now,
                    TipoEvento = evento.Nome,
                    Descricao = evento.GerarDescricao()
                });
            }
        }
        catch (Exception ex) when (ex is not AnaliseClimaticaException)
        {
            throw new AnaliseClimaticaException("Não foi possível analisar os dados climáticos.", ex);
        }
    }
}
