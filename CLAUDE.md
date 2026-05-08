# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Ecosystem Context

This repository is **one of five** in the Mecânica Hermes ecosystem. They live as siblings under
`C:\git\mecanica-hermes\` (or equivalent). When working here, keep in mind:

| Repo | Role | Key tech |
|---|---|---|
| `mecanica-hermes-cadastros` | **This repo** — Clientes e Produtos; recebe webhook de aprovação/rejeição de orçamento | .NET 10, PostgreSQL |
| `mecanica-hermes-api-ordem-servico` | Orquestra ordens de serviço (state machine, SAGA principal) | .NET 10, PostgreSQL, RabbitMQ |
| `mecanica-hermes-api-pagamentos` | Integração Mercado Pago, gera link, recebe webhook MP | .NET 10, MongoDB |
| `mecanica-hermes-tests-e2e` | Suíte E2E Robot Framework BDD cobrindo as 3 APIs em Docker Compose | Python 3.11, Robot Framework, Allure |
| `mecanica-hermes-shared-sdk` | SDK compartilhado em GitHub Packages (6 pacotes NuGet) | .NET 10, MassTransit.Abstractions |

### Cross-service integration

- **Mensageria** (RabbitMQ via MassTransit): 3 eventos versionados via `[EntityName(".v1")]` que
  esta API consome/publica (ver "Cross-service Integration Events" abaixo).
- **HTTP M2M**: esta API **expõe** `GET /api/clientes/{id}` consumido pelos repos de OS e Pagamentos
  via JWT M2M (no Development o `DevelopmentAuthenticationMiddleware` bypassa).
- **Webhook signed**: `POST /api/webhooks/orcamentos/{token}` recebe a aprovação/rejeição do cliente.
  Validação por HMAC via `WEBHOOK__SIGNING_SECRET` + dedup por `webhook_event_id` na tabela
  `webhook_events`.

### Shared SDK status (em adoção)

PR #1 do `mecanica-hermes-shared-sdk` ainda **não foi mergeado** quando este CLAUDE.md foi escrito.
Quando for: 6 pacotes NuGet substituirão ~700 linhas duplicadas entre os 3 repos. Até lá,
**não mover código para o SDK** sem alinhar com o usuário primeiro.

### Diferença frente a OS/Pagamentos

- **Sem Outbox** (decisão registrada em ADR-002 de Cadastros): publicação direta via
  `IPublishEndpoint` após commit do `webhook_events`. Risco aceito: se o RabbitMQ estiver
  indisponível entre o commit e o publish, o evento se perde. Migração para Outbox EF Core
  está prevista na Fase 4 do roadmap (resiliência crítica).
- **Sem SAGA**: este serviço é puramente reativo. Não orquestra estado de máquina.
- **`BaseDomain` estendido com `IsDeleted`**: soft delete. OS/Pagamentos usam a versão sem.

## Commands

```bash
# Build
dotnet build --configuration Release

# Run API locally (requer Postgres rodando)
dotnet run --project src/Mecanica.Hermes.Cadastros.Api

# Run all tests
dotnet test Mecanica.Hermes.Cadastros.slnx

# Run only unit tests (no Docker required)
dotnet test tests/Mecanica.Hermes.Cadastros.Domain.Tests
dotnet test tests/Mecanica.Hermes.Cadastros.Application.Tests
dotnet test tests/Mecanica.Hermes.Cadastros.Api.Tests

# Run tests with coverage
dotnet test Mecanica.Hermes.Cadastros.slnx --settings coverlet.runsettings --results-directory .temp/coverage

# Generate coverage report
reportgenerator -reports:.temp/coverage/**/coverage.opencover.xml -targetdir:.temp/coverage/report -reporttypes:TextSummary;Html

# EF Core migrations
dotnet ef migrations add <Name> --project src/Mecanica.Hermes.Cadastros.Infrastructure --startup-project src/Mecanica.Hermes.Cadastros.Api
dotnet ef database update --project src/Mecanica.Hermes.Cadastros.Infrastructure --startup-project src/Mecanica.Hermes.Cadastros.Api
```

Integration tests (`Mecanica.Hermes.Cadastros.IntegrationTests`) require Docker to spin up a
PostgreSQL container via Testcontainers. Architecture tests
(`Mecanica.Hermes.Cadastros.ArchitectureTests`) enforce dependency rules via NetArchTest.

Coverage threshold enforced in CI: ≥80% line coverage. Atualmente em ~83.8%.

## Architecture

**Clean Architecture + DDD + CQRS**, strict dependency rule: `Domain ← Application ← Infrastructure`,
`Api → all`.

```text
Api (Minimal API)
 └─► Application (CQRS / MediatR)
      └─► Domain (pure business logic)
Infrastructure (EF Core, MassTransit)
 └─► Application (implements ports)
```

### Projects

| Project | Role |
| :--- | :--- |
| `Mecanica.Hermes.Cadastros.Domain` | Aggregates `Cliente`, `Produto`, value objects, `Result<T>`, `BaseDomain` (com `IsDeleted`) |
| `Mecanica.Hermes.Cadastros.Application` | CQRS commands/queries, MassTransit consumers, ports, validators FluentValidation |
| `Mecanica.Hermes.Cadastros.Infrastructure` | EF Core, repositories, MassTransit setup, observability, Webhook handling |
| `Mecanica.Hermes.Cadastros.Api` | Minimal API endpoints, middleware, `ResultPresenter`, OpenAPI/Scalar |
| `Mecanica.Hermes.Cadastros.TestUtils` | Builders Bogus + fakes (não é projeto runnable) |

### Domínio: Cliente e Produto

Dois aggregate roots independentes:

- **`Cliente`** — identificado por `Cpf` (value object validado). Contém `Email`, `Endereco`,
  contato. Soft delete via `IsDeleted`.
- **`Produto`** — catálogo de peças. `Codigo`, `Nome`, `PrecoUnitario`. Snapshot dos preços é
  copiado para a OS no momento da criação, então mudanças aqui não retroagem.

### Webhook de orçamento

Fluxo crítico cross-service:

1. OS publica `ordem-de-servico.aguardando-aprovacao.v1`.
2. Consumer em Cadastros cria um `webhook_link` (token único, expira em 7 dias) e dispara email
   ao cliente com link `https://.../api/webhooks/orcamentos/{token}`.
3. Cliente clica → frontend POSTa para o endpoint público com `decisao` (aprovado/rejeitado).
4. Endpoint:
   - Valida HMAC do header `X-Signature` contra `WEBHOOK__SIGNING_SECRET`.
   - Idempotência via `webhook_event_id` (tabela `webhook_events`).
   - Persiste decisão e publica `orcamento-aprovado-pelo-cliente.v1` ou
     `orcamento-rejeitado-pelo-cliente.v1`.

### Result Pattern

Igual aos repos OS/Pagamentos. `Result`/`Result<T>` carrega `HttpStatusCode`, `IsSuccess`,
`Errors`. `ResultPresenter` na Api converte para `IResult` (RFC 7807 problem details).

**Cadastros adiciona um overload** específico de `Result<T>` (não presente nos outros repos) que
não foi padronizado. Quando o SDK Tier 1 for adotado, esse overload precisa ser revisado para
caber na assinatura compartilhada.

### CQRS Flow

**Write (async commands)** — mesmo padrão dos demais: route handler → command via `ISender` →
`AsyncCommandHandler<T>` publica via `IAsyncMessagePublisher` (MassTransit) → consumer processa.

**Read (sync queries)** — query handlers chamam repositório direto, retornam `Result<T>`.

### Cross-service Integration Events

3 contratos cross-service envolvendo este repo. Todos versionados com `[EntityName(".v1")]`:

| Direção | Evento | Quando |
|---|---|---|
| Consome ← | `ordem-de-servico.aguardando-aprovacao.v1` | OS pediu aprovação — Cadastros gera webhook link |
| Publica → | `orcamento-aprovado-pelo-cliente.v1` | Webhook recebeu aprovação |
| Publica → | `orcamento-rejeitado-pelo-cliente.v1` | Webhook recebeu rejeição |

**Regra de evolução**: nunca alterar a forma de um evento publicado — sempre criar `.v2` em
paralelo e drenar `.v1` antes de removê-lo.

### Endpoint Registration Pattern

Mesma convenção do OS/Pagamentos. Routes são extension methods estáticos:

```csharp
// Program.cs
app.MapClientesEndpoints();
app.MapProdutosEndpoints();
app.MapWebhooksOrcamentosEndpoints();

// Routes/GetClienteByIdRoute.cs
endpoints.MapGet("/{id:guid}", GetById)
    .RequireAuthorization(AuthPolicies.AdminOrM2MScope);
```

### Authentication

JWT Bearer + scopes (`AuthPolicies`). Cadastros tem **`AdminOrM2MScope`** (extensão sobre o
padrão dos outros repos) para liberar `GET /api/clientes/{id}` ao serviço Pagamentos via M2M.
No Development, `DevelopmentAuthenticationMiddleware` bypassa.

### Repository Interfaces (ISP)

- `IClienteReader` / `IClienteWriter` / `IClienteRepository`
- `IProdutoReader` / `IProdutoWriter` / `IProdutoRepository`
- `IWebhookEventRepository` (idempotência de webhooks)

### Key Dependencies

| Concern | Package |
| :--- | :--- |
| CQRS mediator | MediatR 14 |
| Async messaging | MassTransit 8 (RabbitMQ) |
| DI container | Autofac 9 |
| ORM | EF Core 10 + Npgsql |
| Validation | FluentValidation |
| API docs | Scalar + Microsoft.AspNetCore.OpenApi |
| Observability | OpenTelemetry (OTLP → New Relic) + Serilog |
| Testing | xUnit, Moq, FluentAssertions, Bogus, Testcontainers, NetArchTest |

## Testing Strategy

- **`Domain.Tests`** — pure unit tests, sem mocks (domínio é side-effect-free).
- **`Application.Tests`** — unit tests com Moq; consumers via `MassTransit.TestFramework`.
- **`Infrastructure.Tests`** — repositórios contra Testcontainers Postgres.
- **`Api.Tests`** — middleware, presenter, route helpers.
- **`IntegrationTests`** — full HTTP stack via `WebApplicationFactory` + Testcontainers Postgres.
- **`ArchitectureTests`** — NetArchTest forçando regras de dependência (`Domain` não conhece
  `Infrastructure`, etc.).
- **`TestUtils`** — `ClienteBuilder`, `ProdutoBuilder` Bogus-powered.

## Docs

Detailed documentation lives in `docs/`:

- `architecture.md` — Layer dependency diagram and patterns
- `database.md` — ER diagram, indexes, migration conventions
- `messaging.md` — Eventos cross-service, webhook flow
- `api-reference.md` — Endpoints, auth scopes
- `configuration.md` — Environment variables
- `observability.md` — Metrics, traces, logs
- `runbook.md` — Operational incidents
- `testing.md` — Test strategy
- `ci-cd.md` — GitHub Actions and Docker multi-platform build
- `adrs.md` — Architecture Decision Records (ADR-002 documenta a ausência de Outbox)
