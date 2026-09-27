# Regras de desenvolvimento para teste

Estas regras se aplicam a todo o repositório.

## Segurança

- Nunca inclua senhas, tokens ou chaves de API diretamente no código.
- Nunca registre senhas, tokens ou dados de autenticação em logs.
- Consultas SQL com dados externos devem usar parâmetros. Não concatene entradas do usuário no SQL.

## Tratamento de erros

- Não deixe blocos `catch` vazios.
- Não retorne sucesso quando uma operação falhar.
- Valide argumentos obrigatórios antes de executar a operação.

## C# e .NET

- Métodos que retornam `Task` ou `Task<T>` devem ter o sufixo `Async`.
- Não utilize `.Result` ou `.Wait()` para aguardar tarefas em métodos assíncronos.
- Passe o `CancellationToken` recebido para operações assíncronas que aceitem esse parâmetro.
- Utilize `decimal` para valores monetários.

## Testes

- Alterações em regras de negócio devem incluir testes do comportamento alterado.
- Correções de bugs devem incluir um teste que reproduza o problema.

## Orientações para revisão de PRs

- Escreva todos os comentários de revisão de PRs em português do Brasil (pt-BR), incluindo títulos dos apontamentos, explicações, sugestões de correção e resumo da revisão, mesmo quando o código ou a descrição da PR estiverem em inglês.
- Preserve nomes de classes, métodos, variáveis, APIs, caminhos e mensagens de erro citadas no idioma original.

- Identifique violações destas regras nas linhas adicionadas ou alteradas.
- Explique o problema e sugira uma correção concreta.
- Priorize segurança, comportamento incorreto e tratamento de erros.
- Evite comentários sobre preferências de estilo não previstas neste arquivo.
