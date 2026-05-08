# Observabilidade

## OpenTelemetry

A API de Cadastros usa **OpenTelemetry .NET** para traces, métricas e logs. Toda a exportação é controlada pela variável `OBSERVABILITY__ENABLED`. Em `false` (padrão), os exportadores não são registrados e não há overhead de rede.

## Traces

Atividades são criadas via `ActivitySource` registrado em `CadastrosActivitySources`:

- Source name: `Mecanica.Hermes.Cadastros`
- Comportamentos rastreados automaticamente: requisições HTTP de entrada (ASP.NET Core), queries EF Core, chamadas HTTP de saída (HttpClient), mensagens MassTransit

## Métricas

Métricas emitidas pelo `CadastrosMetrics` (meter `Mecanica.Hermes.Cadastros`):

| Métrica | Tipo | Descrição |
| --- | --- | --- |
| `cadastros.clientes.criados` | Counter | Clientes criados com sucesso |
| `cadastros.clientes.atualizados` | Counter | Clientes atualizados |
| `cadastros.clientes.removidos` | Counter | Clientes removidos |
| `cadastros.produtos.criados` | Counter | Produtos criados |
| `cadastros.servicos.criados` | Counter | Serviços criados |
| `cadastros.emails.enviados` | Counter | E-mails enviados via MailKit |
| `cadastros.webhooks.recebidos` | Counter | Webhooks processados (por `acao`: aprovar/rejeitar) |

As métricas são incrementadas pelos **handlers de domain events** via `IDomainEventDispatcher` — disparados após o `CommitAsync` de cada operação.

## Exportadores

Configurados via variáveis de ambiente (prefixo `OTEL_`):

```bash
OBSERVABILITY__ENABLED=true
OTEL_SERVICE_NAME=mecanica-hermes-cadastros-api
OTEL_EXPORTER_OTLP_ENDPOINT=https://otlp.nr-data.net
OTEL_EXPORTER_OTLP_HEADERS=api-key=<new-relic-token>
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
```

Para logs via OTLP separado:

```bash
OTEL_EXPORTER_OTLP_LOGS_ENDPOINT=https://log-api.newrelic.com/log/v1
OTEL_EXPORTER_OTLP_LOGS_HEADERS=api-key=<new-relic-token>
OTEL_EXPORTER_OTLP_LOGS_PROTOCOL=http/protobuf
```

## Logging estruturado

Serilog é configurado com:

- **Console sink** (JSON em produção, pretty-print em dev)
- **OTLP sink** quando `OBSERVABILITY__ENABLED=true`
- Enrichers: `LogContext`, `Environment`, `MachineName`

## Ambiente de desenvolvimento

Em `Development`/`Testing`, o `DevelopmentAuthenticationMiddleware` injeta um usuário autenticado com todos os escopos em cada requisição, eliminando a necessidade de configurar JWT local.
