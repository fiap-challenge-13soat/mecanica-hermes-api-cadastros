# Guia de contribuição

## Antes de começar

1. Leia [architecture.md](./architecture.md) para entender as camadas e a regra de
   dependência (validada pelo `Mecanica.Hermes.Cadastros.ArchitectureTests`).
2. Leia [messaging.md](./messaging.md) — o webhook HMAC e os eventos cross-service são
   o coração da integração com OS e Pagamentos.
3. Familiarize-se com os ADRs em [adrs.md](./adrs.md) — eles cobrem os trade-offs já
   discutidos (Outbox, soft delete, BaseDomain paralelo ao SDK).

## Branching

| Branch | Para quê |
| --- | --- |
| `main` | Linha estável; deploys de HML disparam daqui. |
| `feature/<descrição-curta>` | Trabalho em andamento. Cobertura de testes é executada no push. |
| `versao/<x.y.z>` | Branch de estabilização para release (opcional). |

Tags `v*.*.*` em `main` disparam o pipeline de produção.

## Setup local

```bash
# pré-requisitos: .NET 10, Docker, just (opcional)

dotnet restore
dotnet build --configuration Release

# subir Postgres (ver README para variáveis mínimas)
# rodar a API
just run
```

## Estilo de código

- `Directory.Build.props` define `Nullable=enable`, `ImplicitUsings=enable`,
  `EnableNETAnalyzers=true` (`AnalysisMode=Recommended`).
- `.editorconfig` na raiz é fonte da verdade para indent/quebras/espaços.
- Convenções:
  - Arquivos: `PascalCase.cs`.
  - Namespaces seguem o caminho da pasta.
  - Records `sealed` por default (especialmente para mensagens MassTransit).
  - Use `Result`/`Result<T>` para sinalizar erro de regra de negócio; reserve exceções
    para falhas inesperadas.
  - Não introduza dependências em Domain (validado por testes de arquitetura).

## Formatação

```bash
# aplicar formatação (whitespace + style + analyzers)
just format

# verificar sem alterar (espelha o CI)
just format-check
```

## Workflow de uma mudança típica

### Adicionar uma nova operação em um agregado

1. Definir o método no agregado (`Domain/Clientes/Cliente.cs` ou `Domain/Produtos/Produto.cs`).
2. Adicionar testes em `tests/Mecanica.Hermes.Cadastros.Domain.Tests`.
3. Criar `Command`/`Query` em `Application/Clientes` ou `Application/Produtos`.
4. Criar ou atualizar o handler correspondente.
5. Criar a rota em `Api/Routes/` e registrá-la no endpoint mapper.
6. Cobrir com:
   - testes unitários do handler em `Application.Tests`,
   - integration test em `IntegrationTests` se for end-to-end (HTTP → DB).
7. Atualizar [`api-reference.md`](./api-reference.md).

### Adicionar uma migration

```bash
just migration-add Add{Algo}Para{Entidade}

# revisar o diff gerado em src/Mecanica.Hermes.Cadastros.Infrastructure/Persistence/Migrations
# aplicar localmente:
just migration-apply
```

Não use nomes genéricos como `Migration1`/`Fix`/`Test`. O nome precisa descrever a
mudança (ex.: `AddWebhookEvents`, `AlterClienteAddIsDeleted`). Ver
[database.md](./database.md#convencao-de-nome).

## Pull Requests

- O nome da PR deve descrever a mudança no infinitivo
  (ex.: `feat: adicionar endpoint de busca de produto por código`).
- Inclua: contexto, decisão, alternativas avaliadas (se houver), impacto operacional.
- O CI roda em todo push em `feature/**`. PRs só mergeiam com:
  - build verde,
  - testes verdes,
  - cobertura ≥ 80%.
- Toda mudança em domínio deve vir com testes em `Domain.Tests`.
- Toda mudança em handler/consumer deve vir com testes em `Application.Tests`.
- Mudanças no contrato HTTP devem atualizar `api-reference.md`.

## Convenções de teste

Detalhes em [testing.md](./testing.md). Resumo:

- xUnit + FluentAssertions + Moq + Bogus.
- `ClienteBuilder` / `ProdutoBuilder` do `TestUtils` para criar agregados.
- Naming: `Cenario_Resultado` ou `Verbo_CondicaoEsperada`.
- AAA visível.
- Sem `Thread.Sleep` em testes — use `MassTransit.TestFramework` (`TestHarness`) para
  espera determinística em consumers/BackgroundService.

## Runbook & observabilidade

- Antes de fazer um change com efeito operacional, revise [runbook.md](./runbook.md) para
  entender quais sinais existem.
- Adicionou métrica nova? Documente em [observability.md](./observability.md).

## Segurança

- Endpoints de mutação devem ter `RequireAuthorization(AuthPolicies.OnlyAdminScope)`.
- O endpoint M2M `GET /api/clientes/{id}` usa `AdminOrM2MScope`.
- Nunca comite secrets (`AUTH__*`, `WEBHOOK__SIGNING_SECRET`, etc.). O `launchSettings.json`
  **não** é versionado — é mantido apenas localmente por cada contribuidor.

## Onde achar ajuda

- Documentação: [`docs/`](./).
- Decisões registradas: [`adrs.md`](./adrs.md).
- Testes de arquitetura: `tests/Mecanica.Hermes.Cadastros.ArchitectureTests` — eles
  falham se a regra de dependência é quebrada.
