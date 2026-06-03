using System.Text;
using ClimateSentinel.Models;

namespace ClimateSentinel.Services;

public partial class RelatorioClimatico
{
    public string GerarTexto()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Climate Sentinel - Relatório Climático");
        builder.AppendLine($"Data de geração: {DateTime.Now:dd/MM/yyyy HH:mm}");
        builder.AppendLine(new string('-', 48));
        builder.AppendLine($"Quantidade de consultas: {TotalConsultas}");
        builder.AppendLine($"Quantidade de alertas: {TotalAlertas}");
        builder.AppendLine($"Média de temperatura: {MediaTemperatura:F1} °C");
        builder.AppendLine($"Média de precipitação: {MediaPrecipitacao:F1} mm");
        builder.AppendLine("Eventos detectados:");

        if (EventosDetectados.Count == 0)
        {
            builder.AppendLine("- Nenhum evento extremo detectado.");
        }
        else
        {
            foreach (var evento in EventosDetectados)
            {
                builder.AppendLine($"- {evento}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Detalhamento das consultas:");

        foreach (var consulta in Consultas.OrderByDescending(item => item.DataConsulta))
        {
            builder.AppendLine(
                $"{consulta.DataConsulta:dd/MM/yyyy HH:mm} | {consulta.Cidade.Nome} | " +
                $"Temp: {consulta.DadoClimatico.Temperatura:F1} °C | " +
                $"Umidade: {consulta.DadoClimatico.Umidade:F0}% | " +
                $"Chuva: {consulta.DadoClimatico.Precipitacao:F1} mm | " +
                $"Vento: {consulta.DadoClimatico.VelocidadeVento:F1} km/h");
        }

        return builder.ToString();
    }

    public string SalvarEmArquivo(string caminho)
    {
        var conteudo = GerarTexto();
        File.WriteAllText(caminho, conteudo, Encoding.UTF8);
        return caminho;
    }
}
