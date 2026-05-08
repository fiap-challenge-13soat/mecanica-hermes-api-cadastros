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

### Shared SDK adotado (v1.0.0)

Cadastros consome 5 dos 6 pacotes publicados em GitHub Packages na org `fiap-challenge-13soat`:

| Pacote | Onde é usado |
|---|---|
| `Mecanica.Hermes.Shared.Core` | `Result<T>`, `BaseDomain` (extendido localmente com `IsDeleted`), `IDomainEvent`, `IUnitOfWork`, `ConcurrencyConflictException`, `PaginationParams`, `PagedResult`, `EnvironmentNames` |
| `Mecanica.Hermes.Shared.Application` | `LoggingBehavior` |
| `Mecanica.Hermes.Shared.AspNetCore` | `CorrelationIdMiddleware`, `DevelopmentAuthenticationMiddleware`, `ApiProblemDetails`, `HealthCheckTags` |
| `Mecanica.Hermes.Shared.Observability` | `ActivitySources` (source `MecanicaHermes.Api`) |
| `Mecanica.Hermes.Contracts` | 4 events `.v1` (consome `OrdemDeServicoAguardandoAprovacaoEvent`/`LinkPagamentoGeradoEvent`; publica `OrcamentoAprovadoPeloClienteEvent`/`OrcamentoRejeitadoPeloClienteEvent`) |

`Mecanica.Hermes.Shared.MassTransit` **não é consumido** — Cadastros não usa MassTransit Sagas e o pacote ainda é skeleton/Tier 2.

**Fica local (Tier 2)** porque há divergência intencional do SDK:
- `Domain/Common/Abstractions/BaseDomain.cs` — **classe paralela ao SDK** (não herda). Adiciona `IsDeleted`/`MarkAsDeleted` (soft delete) e diverge nas assinaturas: `Id { internal set; }` (SDK usa `private set`) e `RestaurarBase` com parâmetro extra `bool isDeleted` (SDK tem 3 params). Reusa apenas o `DomainClock` do SDK.
- `Domain/Common/Abstractions/AggregateRoot.cs` — paralela ao SDK também; usa local `BaseDomain` + local `IDomainEvent` (preserva soft delete e MediatR dispatch na hierarquia)
- `Domain/Common/Abstractions/IDomainEvent.cs` — extende `Shared.Core.Domain.IDomainEvent` + `MediatR.INotification` para que o `MediatorDomainEventDispatcher` continue funcionando
- `Application/Common/Persistence/IUnitOfWork.cs` — assina sobre `local.AggregateRoot` (não o do SDK) por causa do soft delete
- `Application/Common/Behaviors/TracingBehavior.cs` — Tier 2; usa source local `MecanicaHermes.Cadastros.Application`
- `Api/Middleware/GlobalExceptionHandlerMiddleware.cs` + `MiddlewareExtensions.UseGlobalExceptionHandler` — switch de exceções específico do repo

### Por que soft delete em Cadastros mas não em OS/Pagamentos

Modelagem de domínio diferente:
- **OS** é uma **state machine** — `Cancelada` é um status terminal no agregado, não um delete (a row sempre existe; o histórico de status é parte da auditoria).
- **Pagamentos** segue lifecycle do Mercado Pago — `Recusado`/`Expirado` são estados, não deletes.
- **Cadastros** é **CRUD com lifecycle de cliente/produto** — deletar um cliente não pode quebrar a referência lógica das OSs antigas (que estão em outro DB, sem FK física). Soft delete preserva o ID + serve como base para os partial unique indexes (`Cpf`/`Email`/`Codigo`/`Nome` únicos apenas entre não-deletados, permitindo recriar entidade depois).

Por isso só Cadastros estende `BaseDomain` + `IUnitOfWork` localmente; OS e Pagamentos consomem direto do SDK.

### nuget.config — credenciais para baixar do SDK

Restore em local/CI requer:

```bash
$env:GITHUB_USERNAME = "<seu-handle>"
$env:GITHUB_TOKEN    = "<PAT classic com read:packages + SSO autorizado para fiap-challenge-13soat>"
dotnet restore
```

**Limitação GitHub conhecida**: fine-grained PATs **não autenticam** contra `nuget.pkg.github.com` — gera 401 independente das permissões marcadas. Use **classic PAT**.

Em CI o `GITHUB_TOKEN` injetado pelo Actions já tem `packages: read` quando a permission do workflow declara — os 4 workflows do repo já estão com isso configurado. No `Dockerfile` os mesmos valores entram via `--build-arg GITHUB_USERNAME=... --build-arg GITHUB_TOKEN=...`, propagados pelo `docker-ci.yml`.

### Diferença frente a OS/Pagamentos

- **Outbox transacional para webhook → OS** (ADR-007): integration events do fluxo de
  aprovação/rejeição de orçamento são enfileirados na tabela `OutboxMessages` no MESMO
  `SaveChanges` do `webhook_events` (atomicidade garantida pelo Postgres). O `OutboxProcessor`
  (BackgroundService) faz polling de 2s, `SELECT … FOR UPDATE SKIP LOCKED` para multi-instância,
  e dispatcha via `IPublishEndpoint`. DLQ após 10 tentativas (configurável via `OUTBOX__MAX_RETRIES`).
  Métricas OTel: `outbox.messages.{dispatched,failed,dead_lettered}` no meter
  `MecanicaHermes.Cadastros.Outbox`. ADR-002 ainda vale para domain events in-process
  (best-effort, sem efeito cross-service).
- **DLQ observability** (`Infrastructure/Messaging/MessagingFaultObserver`): `IConsumeObserver`
  global hooka `ConsumeFault<T>` depois de retries esgotados — emite counter
  `messaging.consume_faults` com tags `message_type`/`exception_type` no meter
  `MecanicaHermes.Cadastros.Messaging`. Use pra alarmar quando volume de DLQ cresce.
- **Sem SAGA**: este serviço é puramente reativo. Não orquestra estado de máquina.
- **`BaseDomain` paralelo com `IsDeleted`**: soft delete real (não vestígio). Local `BaseDomain` é classe paralela ao SDK (não herda — assinaturas divergem). OS/Pagamentos usam direto o do SDK.

### Test infra de mensageria

`MassTransit.TestFramework` está disponível em `Mecanica.Hermes.Cadastros.Infrastructure.Tests`.
Use `services.AddMassTransitTestHarness()` + `await harness.Published.Any<T>(ct)` quando precisar
de **espera determinística** por publicação no bus (substitui `Task.Delay(N)` flaky em testes
de BackgroundService). Exemplo aplicado em
`OutboxProcessorTests.ProcessBatch_CaminhoFeliz_DeveMarcarStatusProcessedAposDespacho`.

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
