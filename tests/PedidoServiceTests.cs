using System.Net;
using Microsoft.Data.Sqlite;
using Xunit;

namespace TesteReviewAutomatico.Tests;

public sealed class PedidoServiceTests
{
    [Fact]
    public void CalcularTotalPreservaPrecisaoMonetaria()
    {
        using var client = new HttpClient();
        var service = new PedidoService(client);
        Assert.Equal(0.3m, service.CalcularTotal(0.1m, 3));
        Assert.Equal(0m, service.CalcularTotal(10m, 0));
        Assert.Equal(0m, service.CalcularTotal(0m, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => service.CalcularTotal(-1m, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => service.CalcularTotal(1m, -1));
    }

    [Fact]
    public async Task SalvarPedidoPropagaFalhaDeGravacaoAsync()
    {
        using var client = new HttpClient();
        var service = new PedidoService(client);
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "pedido.txt");
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => service.SalvarPedidoAsync(path, "pedido"));
    }

    [Fact]
    public async Task SalvarPedidoPersisteConteudoAsync()
    {
        using var client = new HttpClient();
        var path = Path.GetTempFileName();
        try
        {
            Assert.True(await new PedidoService(client).SalvarPedidoAsync(path, "pedido"));
            Assert.Equal("pedido", await File.ReadAllTextAsync(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ExcluirPedidoTrataSqlComoDadoAsync()
    {
        using var client = new HttpClient();
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        using var setup = connection.CreateCommand();
        setup.CommandText = "CREATE TABLE Pedidos (Id TEXT); INSERT INTO Pedidos VALUES ('1'), ('2');";
        await setup.ExecuteNonQueryAsync();
        var service = new PedidoService(client);
        Assert.Equal(0, await service.ExcluirPedidoAsync(connection, "' OR 1=1 --", CancellationToken.None));
        Assert.Equal(1, await service.ExcluirPedidoAsync(connection, "1", CancellationToken.None));
        using var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM Pedidos";
        Assert.Equal(1L, await count.ExecuteScalarAsync());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ExcluirPedidoAsync(connection, "2", cancellation.Token));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ConsultarPedidoRejeitaIdAusenteAsync(string? id)
    {
        using var client = new HttpClient();
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            new PedidoService(client).ConsultarPedidoAsync(id!, CancellationToken.None));
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    public async Task ConsultarPedidoRejeitaSegmentosRelativosSemEnviarRequisicaoAsync(string id)
    {
        using var handler = new ResponseHandler();
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid/") };
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            new PedidoService(client).ConsultarPedidoAsync(id, CancellationToken.None));
        Assert.Equal("pedidoId", exception.ParamName);
        Assert.Null(handler.RequestPath);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("pedido.123")]
    public async Task ConsultarPedidoRetornaConteudoAsync(string id)
    {
        using var handler = new ResponseHandler();
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid/") };
        Assert.Equal("pedido", await new PedidoService(client).ConsultarPedidoAsync(id, CancellationToken.None));
        Assert.Equal("/pedidos/" + id, handler.RequestPath);
    }

    [Fact]
    public async Task ConsultarPedidoPropagaCancelamentoAsync()
    {
        using var handler = new WaitingHandler();
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid/") };
        using var cancellation = new CancellationTokenSource();
        var request = new PedidoService(client).ConsultarPedidoAsync("123", cancellation.Token);
        await handler.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    private sealed class ResponseHandler : HttpMessageHandler
    {
        public string? RequestPath { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestPath = request.RequestUri!.AbsolutePath;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("pedido") });
        }
    }

    private sealed class WaitingHandler : HttpMessageHandler
    {
        public TaskCompletionSource<bool> Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Started.TrySetResult(true);
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
