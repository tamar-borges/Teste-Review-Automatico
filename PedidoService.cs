using System;
using System.Data.Common;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TesteReviewAutomatico;

public sealed class PedidoService
{
    private readonly HttpClient httpClient;

    public PedidoService(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        this.httpClient = httpClient;
    }

    public decimal CalcularTotal(decimal precoUnitario, int quantidade)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(precoUnitario);
        ArgumentOutOfRangeException.ThrowIfNegative(quantidade);
        return precoUnitario * quantidade;
    }

    public async Task<string> ConsultarPedidoAsync(string pedidoId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pedidoId);
        using var response = await httpClient.GetAsync(
            "pedidos/" + Uri.EscapeDataString(pedidoId), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<bool> SalvarPedidoAsync(
        string caminho, string conteudo, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caminho);
        ArgumentNullException.ThrowIfNull(conteudo);
        await File.WriteAllTextAsync(caminho, conteudo, cancellationToken);
        return true;
    }

    public async Task<int> ExcluirPedidoAsync(
        DbConnection connection,
        string pedidoId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(pedidoId);
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Pedidos WHERE Id = @pedidoId";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@pedidoId";
        parameter.Value = pedidoId;
        command.Parameters.Add(parameter);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
