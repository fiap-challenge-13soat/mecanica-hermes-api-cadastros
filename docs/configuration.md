# Configuração

Todas as variáveis usam **double-underscore** (`__`) para mapeamento ASP.NET Core (`IConfiguration`).

## NuGet — GitHub Packages (SDK)

| Variável | Default | Obrigatório | Notas |
| --- | --- | --- | --- |
| `GITHUB_USERNAME` | — | **Build/restore** | Handle GitHub do dev (não email). Lido por `nuget.config` para autenticar em `nuget.pkg.github.com/fiap-challenge-13soat`. Em CI vem de `${{ github.actor }}`. |
| `GITHUB_TOKEN` | — | **Build/restore** | PAT **classic** com escopo `read:packages` + SSO autorizado para a org. Fine-grained PATs **não funcionam** com GitHub Packages NuGet (limitação atual da plataforma). Em CI vem de `${{ secrets.GITHUB_TOKEN }}`. |

> Em runtime (após o build/publish), nenhuma dessas variáveis é necessária — os pacotes já estão restored e empacotados. Só são exigidas durante `dotnet restore`/`dotnet build` (local, CI ou Dockerfile).

## Banco de dados

| Variável | Default | Obrigatório | Notas |
| --- | --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | — | **Sim** | Connection string PostgreSQL. Ex.: `Host=localhost;Database=mecanica_hermes_cadastros;Username=postgres;Password=postgres` |

## Autenticação JWT

| Variável | Default | Obrigatório | Notas |
| --- | --- | --- | --- |
| `AUTH__AUTHORITY` | — | **Sim** | OIDC authority / well-known endpoint |
| `AUTH__ISSUER` | — | **Sim** | Valor esperado do claim `iss` |
| `AUTH__CLIENTE_SCOPE` | `cliente-scope` | Não | Scope que libera leitura de cadastros |
| `AUTH__ADMIN_SCOPE` | `admin-scope` | Não | Scope que libera mutação de cadastros |
| `AUTH__SERVICO_SCOPE` | `servico-scope` | Não | Scope que permite acesso M2M da API de Pagamentos ao `GET /api/clientes/{id}` |

> Em ambiente `Development` / `Testing`, o `DevelopmentAuthenticationMiddleware` bypassa a validação JWT. Nenhuma configuração de auth é necessária nesses ambientes.

## E-mail (SMTP via MailKit)

| Variável | Default | Obrigatório | Notas |
| --- | --- | --- | --- |
| `EmailSender__Enabled` | `false` | Não | `true` para ativar envio real via SMTP |
| `EmailSender__SmtpServer` | — | Se habilitado | Host SMTP (ex.: `smtp.gmail.com`) |
| `EmailSender__SmtpPort` | `25` | Não | Porta SMTP |
| `EmailSender__SslRequired` | `false` | Não | Exigir TLS na conexão SMTP |
| `EmailSender__UserName` | — | Não | Usuário para autenticação SMTP |
| `EmailSender__Password` | — | Não | Senha para autenticação SMTP |
| `EmailSender__SenderInformation__Name` | — | Se habilitado | Nome exibido no campo "De" |
| `EmailSender__SenderInformation__Address` | — | Se habilitado | E-mail exibido no campo "De" |

## Mensageria (MassTransit / RabbitMQ)

| Variável | Default | Obrigatório | Notas |
| --- | --- | --- | --- |
| `MASSTRANSIT__TRANSPORT_SERVICE` | `""` (in-memory) | Não | `RabbitMq` para usar broker real |
| `MASSTRANSIT__CONNECTION_STRING` | — | Se RabbitMq | URI AMQP. Ex.: `amqp://guest:guest@localhost:5672` |
| `MASSTRANSIT__QUEUE_NAME_PREFIX` | `mecanica-hermes-cadastros` | Não | Prefixo de todas as filas/exchanges |

## HTTP Clients externos

| Variável | Default | Obrigatório | Notas |
| --- | --- | --- | --- |
| `ORDEM_SERVICO_API__BASE_URL` | — | **Sim** (fases 2+) | Base URL da API de OS. Ex.: `http://localhost:5001` |
| `PAGAMENTO_API__BASE_URL` | — | **Sim** (fase 3+) | Base URL da API de Pagamentos. Ex.: `http://localhost:5002` |

## Webhook

| Variável | Default | Obrigatório | Notas |
| --- | --- | --- | --- |
| `WEBHOOK__SIGNING_SECRET` | — | **Sim** (fase 4+) | Segredo HMAC-SHA256 (mínimo 32 bytes). Usado para assinar e validar links de aprovação/rejeição |
| `APP__BASE_URL` | `http://localhost:5000` | Não | URL base pública desta API. Usada para construir os links nos e-mails enviados ao cliente |

## Observabilidade

| Variável | Default | Obrigatório | Notas |
| --- | --- | --- | --- |
| `OBSERVABILITY__ENABLED` | `false` | Não | `true` para ativar exportadores OTLP |
| `OTEL_SERVICE_NAME` | `mecanica-hermes-cadastros-api` | Não | Resource attribute `service.name` |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | — | Se habilitado | Endpoint OTLP HTTP. Ex.: `https://otlp.nr-data.net` |
| `OTEL_EXPORTER_OTLP_HEADERS` | — | Se habilitado | Headers do exporter. Ex.: `api-key=<token>` |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | `http/protobuf` | Não | `grpc` ou `http/protobuf` |
| `OTEL_EXPORTER_OTLP_LOGS_ENDPOINT` | — | Não | Endpoint separado para logs (se diferente do traces) |
| `OTEL_EXPORTER_OTLP_LOGS_HEADERS` | — | Não | Headers para o exporter de logs |
| `OTEL_EXPORTER_OTLP_LOGS_PROTOCOL` | — | Não | Protocolo para o exporter de logs |

## Migrations

| Variável | Default | Notas |
| --- | --- | --- |
| `RUN_MIGRATIONS_ON_STARTUP` | `true` | Aplica migrations pendentes no boot. Desabilitar em produção se preferir migração manual |

## Exemplo de configuração mínima para dev

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Database=mecanica_hermes_cadastros;Username=postgres;Password=postgres"
export AUTH__AUTHORITY=https://test
export AUTH__ISSUER=https://test
export EmailSender__Enabled=false
export WEBHOOK__SIGNING_SECRET=meu-segredo-de-32-bytes-ou-mais-aqui
export APP__BASE_URL=http://localhost:5000
export MASSTRANSIT__TRANSPORT_SERVICE=RabbitMq
export MASSTRANSIT__CONNECTION_STRING=amqp://guest:guest@localhost:5672
export ORDEM_SERVICO_API__BASE_URL=http://localhost:5001
export PAGAMENTO_API__BASE_URL=http://localhost:5002
```
