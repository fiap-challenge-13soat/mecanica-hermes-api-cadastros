# Mecânica Hermes — API de Cadastros

Microsserviço responsável pelo cadastro de **Clientes** (com seus **Veículos**) e de **Produtos** / **Serviços**, além do envio de **e-mails** ao cliente via MailKit. Faz parte da decomposição da API monolítica original em três microsserviços (junto com `mecanica-hermes-api-ordem-servico` e `mecanica-hermes-api-pagamentos`).

## Stack

- **.NET 10** (Minimal APIs)
- **PostgreSQL 18** + **EF Core 10**
- **MediatR** (CQRS) + **AutoMapper** + **Serilog** + **OpenTelemetry**
- **MailKit** (SMTP)
- **Scalar** (UI OpenAPI)
- **xUnit** + **Testcontainers** + **NetArchTest**

## Estrutura (Clean Architecture)

```
src/
  Mecanica.Hermes.Cadastros.Domain/          Aggregates, Value Objects, Domain Events
  Mecanica.Hermes.Cadastros.Application/     Commands/Queries (MediatR), DTOs, Ports
  Mecanica.Hermes.Cadastros.Infrastructure/  EF Core + Postgres, MailKit, Repositories
  Mecanica.Hermes.Cadastros.Api/             Minimal APIs, Endpoints, Auth, Scalar
tests/
  Mecanica.Hermes.Cadastros.{Domain,Application,Infrastructure,Api}.Tests/
  Mecanica.Hermes.Cadastros.IntegrationTests/   (WebApplicationFactory + Testcontainers)
  Mecanica.Hermes.Cadastros.ArchitectureTests/  (NetArchTest)
  Mecanica.Hermes.Cadastros.TestUtils/
```

## Endpoints (resumo)

- `GET/POST/PUT/DELETE /api/clientes` (com sub-rotas `/{id}/veiculos`)
- `GET/POST/PUT/DELETE /api/produtos`
- `GET/POST/PUT/DELETE /api/servicos`
- `GET /healthz/live`, `GET /healthz/ready`, `GET /health`
- `GET /scalar/v1` (UI OpenAPI)

A documentação completa fica disponível na UI Scalar quando a aplicação está rodando.

## Quickstart local

```bash
# Subir Postgres
docker run -d --name pg-cadastros -p 5432:5432 \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=mecanica_hermes_cadastros \
  postgres:18.1

# Configurar variáveis mínimas
export ConnectionStrings__DefaultConnection="Host=localhost;Database=mecanica_hermes_cadastros;Username=postgres;Password=postgres"
export AUTH__AUTHORITY=https://test
export AUTH__ISSUER=https://test
export EmailSender__Enabled=false   # desabilita SMTP em dev

# Build + Run (a migration é aplicada no startup)
dotnet run --project src/Mecanica.Hermes.Cadastros.Api
```

Acesse a UI em `http://localhost:5xxx/scalar/v1`.

## Variáveis de ambiente

| Variável | Default | Descrição |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | (obrigatório) | Connection string do PostgreSQL |
| `AUTH__AUTHORITY` | (obrigatório) | OIDC authority |
| `AUTH__ISSUER` | (obrigatório) | JWT issuer esperado |
| `AUTH__CLIENTE_SCOPE` | `cliente-scope` | Escopo de leitura |
| `AUTH__ADMIN_SCOPE` | `admin-scope` | Escopo de mutação |
| `EmailSender__Enabled` | `false` | Liga/desliga envio SMTP |
| `EmailSender__SmtpServer` | — | Host SMTP |
| `EmailSender__SmtpPort` | `25` | Porta SMTP |
| `EmailSender__SslRequired` | `false` | TLS no SMTP |
| `EmailSender__UserName` / `__Password` | — | Credenciais SMTP (opcional) |
| `EmailSender__SenderInformation__Name` / `__Address` | — | Remetente |
| `RUN_MIGRATIONS_ON_STARTUP` | `true` | Aplica migrations no boot |
| `RATE_LIMIT_PERMIT_PER_MINUTE` | `60` | Rate limit por IP |
| `CORS_ALLOWED_ORIGINS` | (prod) | CSV de origens permitidas |
| `OBSERVABILITY__ENABLED` | `false` | Liga OTel exporter |
| `OTEL_EXPORTER_OTLP_*` | — | Configuração padrão OTel |

## Migrations

```bash
dotnet ef migrations add <Nome> \
  --project src/Mecanica.Hermes.Cadastros.Infrastructure \
  --startup-project src/Mecanica.Hermes.Cadastros.Api
```

## Testes

```bash
dotnet test Mecanica.Hermes.Cadastros.slnx --settings coverlet.runsettings
```

Os IntegrationTests usam Testcontainers — exigem Docker rodando.

## Docker

```bash
docker build -t mecanica-hermes-cadastros:dev .
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e AUTH__AUTHORITY=... -e AUTH__ISSUER=... \
  mecanica-hermes-cadastros:dev
```

## Roadmap

Esta entrega é a **Fase 1** do microsserviço (CRUD + serviço de e-mail). Ainda virão:

- **Fase 2** — Endpoint REST e Consumer para envio de orçamento ao cliente (integração com a API de Ordem de Serviço)
- **Fase 3** — Endpoint REST e Consumer para envio de link de pagamento (integração com a API de Pagamentos)
- **Fase 4** — WebHook público para receber a aprovação/rejeição do orçamento (assinatura HMAC + idempotência + publish no bus, padrão da API de Pagamentos)
