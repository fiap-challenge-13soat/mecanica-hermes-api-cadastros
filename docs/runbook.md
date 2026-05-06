# Runbook operacional

## Health checks

```bash
curl http://<host>/health
curl http://<host>/healthz/live
curl http://<host>/healthz/ready
```

`/healthz/ready` retorna `Unhealthy` se o banco PostgreSQL não estiver acessível. Use para saber se a instância pode receber tráfego.

---

## Problema: webhook HMAC retorna 400 (token inválido ou expirado)

**Sintomas:** cliente clica no link do e-mail e vê "Orçamento inválido ou link expirado".

**Causas possíveis:**

1. **Link expirado:** o parâmetro `ts` (Unix timestamp) está fora da janela de ±30 dias
2. **Segredo incorreto:** `WEBHOOK__SIGNING_SECRET` foi alterado após o link ser gerado
3. **URL corrompida:** o cliente copiou parcialmente o link (parâmetros `token` ou `ts` truncados)

**Diagnóstico:**

```bash
# Verificar quando o link foi gerado (ts em Unix time)
date -d @<valor_do_ts>

# Comparar com agora
date +%s
```

**Resolução:**

- Se expirado: use o endpoint `POST /api/notificacoes/orcamento/{id}/enviar-aprovacao` para reenviar um novo e-mail com link atualizado
- Se segredo alterado: verificar se `WEBHOOK__SIGNING_SECRET` foi rotacionado intencionalmente; se sim, todos os links gerados antes da rotação são inválidos

---

## Problema: e-mail não enviado

**Sintomas:** cliente não recebe e-mail após avançar para `AguardandoAprovacao` ou após gerar link de pagamento.

**Diagnóstico:**

1. Verificar se `EmailSender__Enabled` é `true`
2. Verificar logs da aplicação por erros de conexão SMTP
3. Verificar a dead-letter queue (DLQ) no RabbitMQ Management UI (`/queues`) por mensagens `EnviarEmailCommand` em `_skipped` ou `_error`

```bash
# Verificar DLQ via API de gerenciamento RabbitMQ
curl -u guest:guest http://localhost:15672/api/queues/%2F/mecanica-hermes-cadastros_error
```

**Resolução:**

- Se DLQ: corrija a configuração SMTP, republique as mensagens da DLQ via Management UI
- Se SMTP temporariamente fora: MassTransit aplica retry automático (3×) + redelivery exponencial antes de mover para DLQ

---

## Problema: consumer parado / mensagens acumulando

**Sintomas:** eventos do OS chegam (exchanges têm mensagens) mas não são processados.

**Diagnóstico:**

```bash
# Verificar consumers ativos via Management UI
curl -u guest:guest http://localhost:15672/api/consumers

# Verificar filas com mensagens acumuladas
curl -u guest:guest http://localhost:15672/api/queues | jq '.[] | select(.messages > 0) | {name, messages}'
```

**Resolução:**

1. Reiniciar a instância da API (os consumers reiniciam junto)
2. Se mensagens na DLQ precisam ser reprocessadas: republique via RabbitMQ Management UI (Move messages)
3. Verificar logs para a causa raiz (erro de dependência, banco fora do ar, etc.)

---

## Procedimento: rodar migrations manualmente

```bash
dotnet ef database update \
  --project src/Mecanica.Hermes.Cadastros.Infrastructure \
  --startup-project src/Mecanica.Hermes.Cadastros.Api \
  --connection "Host=<host>;Database=mecanica_hermes_cadastros;Username=<user>;Password=<pass>"
```

Para ver quais migrations estão pendentes:

```bash
dotnet ef migrations list \
  --project src/Mecanica.Hermes.Cadastros.Infrastructure \
  --startup-project src/Mecanica.Hermes.Cadastros.Api
```
