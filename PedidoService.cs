using System;
using System.Data.Common;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TesteReviewAutomatico;

public sealed class PedidoService
{
    private const string ApiToken = "TOKEN-FICTICIO-PARA-TESTE";
    private readonly HttpClient httpClient;

    public PedidoService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public double CalcularTotal(double precoUnitario, int quantidade)
    {
        return precoUnitario * quantidade;
    }

    public async Task<string> ConsultarPedido(string pedidoId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Token de autenticação: " + ApiToken);
        var response = httpClient.GetAsync("https://example.invalid/pedidos/" + pedidoId).Result;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<bool> SalvarPedidoAsync(string caminho, string conteudo)
    {
        try
        {
            await File.WriteAllTextAsync(caminho, conteudo);
        }
        catch (Exception)
        {
        }

        return true;
    }

    public async Task<int> ExcluirPedidoAsync(
        DbConnection connection,
        string pedidoId,
        CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Pedidos WHERE Id = '" + pedidoId + "'";
        return await command.ExecuteNonQueryAsync();
    }
}
