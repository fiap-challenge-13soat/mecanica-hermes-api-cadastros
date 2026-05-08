# Testes

## Estratégia

| Tipo | Projeto | Foco | Meta de cobertura |
| --- | --- | --- | --- |
| **Unitário — Domínio** | `Domain.Tests` | Value Objects, Aggregates, Domain Events | 100% das regras de negócio |
| **Unitário — Application** | `Application.Tests` | Command/Query Handlers, Consumers MassTransit | ≥ 80% de linha |
| **Unitário — API** | `Api.Tests` | Presenters, Mappings, Validators | ≥ 80% de linha |
| **Unitário — Infrastructure** | `Infrastructure.Tests` | VoHelpers, Mappers (EF↔Domain), HmacTokenSigner, UrlBuilder | ≥ 80% de linha |
| **Arquitetura** | `ArchitectureTests` | Dependências entre camadas (NetArchTest) | 100% das regras |
| **Integração** | `IntegrationTests` | WebApplicationFactory + Testcontainers (PostgreSQL) | — |

> Os testes de integração (`IntegrationTests`) usam **Testcontainers** para subir um PostgreSQL real. Exigem Docker rodando localmente.

## Projetos

```
tests/
  Mecanica.Hermes.Cadastros.Domain.Tests/         # 127 testes
  Mecanica.Hermes.Cadastros.Application.Tests/    # 84 testes
  Mecanica.Hermes.Cadastros.Api.Tests/            # 56 testes
  Mecanica.Hermes.Cadastros.Infrastructure.Tests/ # 35 testes
  Mecanica.Hermes.Cadastros.ArchitectureTests/    # 7 regras
  Mecanica.Hermes.Cadastros.IntegrationTests/     # (esqueleto — sem testes ativos)
  Mecanica.Hermes.Cadastros.TestUtils/            # Builders e helpers compartilhados
```

## Executar localmente

### Todos os testes (exceto integração)

```bash
dotnet test Mecanica.Hermes.Cadastros.slnx \
  --settings coverlet.runsettings \
  --results-directory .temp/coverage \
  --logger:"console;verbosity=normal"
```

### Apenas unit tests (sem Docker)

```bash
dotnet test Mecanica.Hermes.Cadastros.slnx \
  --filter "FullyQualifiedName!~IntegrationTests" \
  --settings coverlet.runsettings
```

### Gerar relatório HTML de cobertura

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool

reportgenerator \
  -reports:".temp/coverage/**/coverage.opencover.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:"Html;TextSummary" \
  -assemblyfilters:"+Mecanica.Hermes.Cadastros.Api;+Mecanica.Hermes.Cadastros.Application;+Mecanica.Hermes.Cadastros.Domain;+Mecanica.Hermes.Cadastros.Infrastructure;-*Tests"

open coverage-report/index.html
```

## Padrões de teste

- **Builders** em `TestUtils` constroem objetos de domínio com valores válidos por padrão; `With*()` para personalizar
- **FluentAssertions** para assertivas legíveis
- **Moq** para mocks de ports e serviços externos
- **MassTransit.TestFramework** (`InMemoryTestHarness` / `AddMassTransitTestHarness`) para testar consumers sem broker real
- Domain tests **não usam mocks** — testam o agregado e value objects diretamente
