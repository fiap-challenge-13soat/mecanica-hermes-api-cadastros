# Mecânica Hermes — API de Cadastros

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-336791)](https://www.postgresql.org/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-4-FF6600)](https://www.rabbitmq.com/)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=fiap-challenge-13soat_mecanica-hermes-cadastros&metric=alert_status&token=71b077675868d45c85c3f6a512e5ac8d9cc2f8d2)](https://sonarcloud.io/summary/new_code?id=fiap-challenge-13soat_mecanica-hermes-cadastros)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=fiap-challenge-13soat_mecanica-hermes-cadastros&metric=coverage&token=71b077675868d45c85c3f6a512e5ac8d9cc2f8d2)](https://sonarcloud.io/summary/new_code?id=fiap-challenge-13soat_mecanica-hermes-cadastros)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=fiap-challenge-13soat_mecanica-hermes-cadastros&metric=sqale_rating&token=71b077675868d45c85c3f6a512e5ac8d9cc2f8d2)](https://sonarcloud.io/summary/new_code?id=fiap-challenge-13soat_mecanica-hermes-cadastros)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=fiap-challenge-13soat_mecanica-hermes-cadastros&metric=reliability_rating&token=71b077675868d45c85c3f6a512e5ac8d9cc2f8d2)](https://sonarcloud.io/summary/new_code?id=fiap-challenge-13soat_mecanica-hermes-cadastros)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=fiap-challenge-13soat_mecanica-hermes-cadastros&metric=security_rating&token=71b077675868d45c85c3f6a512e5ac8d9cc2f8d2)](https://sonarcloud.io/summary/new_code?id=fiap-challenge-13soat_mecanica-hermes-cadastros)

---

Microsserviço responsável pelo cadastro de **Clientes** (com seus **Veículos**) e de **Produtos** / **Serviços**, além do envio de **e-mails** ao cliente via MailKit e do **webhook** de aprovação/rejeição de orçamento. Faz parte do ecossistema composto por três microsserviços:

| Serviço | Repositório |
| --- | --- |
| **Cadastros** (este) | `mecanica-hermes-cadastros` |
| Ordem de Serviço | `mecanica-hermes-api-ordem-servico` |
| Pagamentos | `mecanica-hermes-api-pagamentos` |

## Stack

- **.NET 10** (Minimal APIs)
- **PostgreSQL 18** + **EF Core 10**
- **MediatR** (CQRS) + **MapperExtensions** estáticos + **Serilog** + **OpenTelemetry**
- **MailKit** (SMTP) + **MassTransit** (RabbitMQ ou in-memory)
- **Scalar** (UI OpenAPI)
- **xUnit** + **Testcontainers** + **NetArchTest**

## Estrutura (Clean Architecture)

```text
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

## Pré-requisitos

| Ferramenta | Versão | Necessidade |
| --- | --- | --- |
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0+ | obrigatória |
| [Docker](https://www.docker.com/get-started) | qualquer atual | testes de integração + serviços de dev |
| [just](https://github.com/casey/just) | qualquer | opcional — atalhos de build/test/migrations |

## Início rápido

### 1. Subir as dependências de runtime

```bash
docker run -d --name pg-cadastros -p 5432:5432 \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=mecanica_hermes_cadastros \
  postgres:18.1
```

### 2. Configurar variáveis mínimas

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Database=mecanica_hermes_cadastros;Username=postgres;Password=postgres"
export AUTH__AUTHORITY=https://test
export AUTH__ISSUER=https://test
export EmailSender__Enabled=false
export WEBHOOK__SIGNING_SECRET=meu-segredo-de-32-bytes-ou-mais-aqui
```

### 3. Rodar a API

```bash
# migrations aplicadas automaticamente no startup
dotnet run --project src/Mecanica.Hermes.Cadastros.Api

# ou via just
just run
```

Acesse a UI interativa em `http://localhost:<porta>/scalar/v1`.

### 4. Rodar os testes

```bash
# tudo (requer Docker — testes de integração usam Testcontainers)
just test

# apenas unitários (sem Docker)
just test-unit
```

Cobertura mínima exigida pelo CI: **≥ 80%** de linhas. Veja [`docs/testing.md`](./docs/testing.md) para detalhes.

## Docker

```bash
docker build -t mecanica-hermes-cadastros:dev .
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e AUTH__AUTHORITY=https://test \
  -e AUTH__ISSUER=https://test \
  -e WEBHOOK__SIGNING_SECRET=meu-segredo-aqui \
  mecanica-hermes-cadastros:dev
```

Imagem pública: `mechermes/mecanica-hermes-cadastros`

## Integrações

| Direção | Com quem | Como |
| --- | --- | --- |
| **Consume** eventos de | API de OS | `OrdemDeServicoAguardandoAprovacaoEvent` → envia e-mail de orçamento |
| **Consume** eventos de | API de Pagamentos | `LinkPagamentoGeradoEvent` → envia e-mail com link de pagamento |
| **Publica** eventos para | API de OS | `OrcamentoAprovadoPeloClienteEvent` / `OrcamentoRejeitadoPeloClienteEvent` via webhook |
| **Expõe** endpoint M2M para | API de Pagamentos | `GET /api/clientes/{id}` com `servico-scope` |

Para o fluxo completo, consulte [`docs/messaging.md`](./docs/messaging.md).

## Documentação

| Documento | Conteúdo |
| --- | --- |
| [`docs/architecture.md`](./docs/architecture.md) | Camadas, padrões, integrações externas e ADRs |
| [`docs/messaging.md`](./docs/messaging.md) | MassTransit, consumers, eventos publicados, webhook HMAC |
| [`docs/api-reference.md`](./docs/api-reference.md) | Endpoints, escopos de autenticação |
| [`docs/configuration.md`](./docs/configuration.md) | Todas as variáveis de ambiente |
| [`docs/database.md`](./docs/database.md) | Tabelas PostgreSQL, migrations |
| [`docs/testing.md`](./docs/testing.md) | Estratégia de testes, projetos, cobertura |
| [`docs/contributing.md`](./docs/contributing.md) | Guia para novos contribuidores |
| [`docs/observability.md`](./docs/observability.md) | OpenTelemetry, métricas, logs |
| [`docs/ci-cd.md`](./docs/ci-cd.md) | GitHub Actions, secrets, deploy |
| [`docs/runbook.md`](./docs/runbook.md) | Diagnóstico e resolução de problemas |
| [`docs/adrs.md`](./docs/adrs.md) | Decisões de arquitetura (ADRs 001–006) |
