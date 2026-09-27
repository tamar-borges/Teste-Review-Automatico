# Executar os testes

Requer o SDK .NET 8:

```powershell
dotnet test tests/TesteReviewAutomatico.Tests.csproj
```

O `PedidoService` recebe um `HttpClient` configurado pelo chamador com a
`BaseAddress` da API. Se a API exigir autenticação, configure-a no cliente
usando credenciais obtidas de configuração segura, sem incluí-las no código
ou nos logs. Os testes HTTP usam um handler local e não acessam uma API real.

Falhas ao salvar pedidos são propagadas como exceções. O retorno `true`
indica que a gravação terminou com sucesso.
