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

## Testar a revisão automática

Após um push para uma branch com PR aberta, confira se a revisão automática
analisou o SHA do commit mais recente. Durante esse teste, não publique
`@codex review`, para verificar exclusivamente o gatilho automático de push.
