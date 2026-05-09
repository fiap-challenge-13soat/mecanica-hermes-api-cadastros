# Decisões de Arquitetura (ADRs)

## ADR-001 — Sem SAGA

**Contexto:** A API de OS usa uma SAGA MassTransit para serializar operações por `OrdemDeServicoId`. Precisávamos decidir se Cadastros seguiria o mesmo padrão.

**Decisão:** Sem SAGA. O fluxo de Cadastros é linear: evento MassTransit recebido → command MediatR → handler → e-mail. Não há operações concorrentes por entidade que precisem de serialização.

**Consequências:** Código mais simples; sem dependência de MongoDB para estado de SAGA; sem timeout sentinel. Adequado para este perfil de uso.

---

## ADR-002 — Sem Outbox transacional para Domain Events in-process

**Contexto:** A API de OS persiste domain events no Outbox na mesma transação do aggregate, garantindo at-least-once delivery. Cadastros tem domain events (ex.: `ClienteCriadoEvent`) que incrementam métricas OTel.

**Decisão:** Sem Outbox para os domain events in-process. Eles são despachados diretamente pelo `IDomainEventDispatcher` (MediatR) após o `CommitAsync`. Se a aplicação cair entre o commit e o dispatch, os eventos são perdidos.

**Escopo desta decisão:** Apenas os eventos in-process (sem efeito cross-service) — `ClienteCriadoEvent`, `ProdutoAlteradoEvent` etc. Os integration events do fluxo webhook → OS ficam fora deste ADR e foram tratados por **ADR-007 (Outbox para webhook → OS)**.

**Por quê aceitamos:** As métricas OTel são best-effort — perder um counter ocasionalmente é aceitável. Implementar o Outbox para domain events in-process adicionaria custo sem benefício proporcional.

**Revisão:** Se Cadastros passar a ter domain events in-process com efeitos colaterais críticos (ex.: cobranças, notificações obrigatórias), reconsiderar.

---

## ADR-003 — MapperExtensions em vez de AutoMapper

**Contexto:** AutoMapper é amplamente usado mas depende de reflexão em runtime, tornando erros de mapeamento silenciosos até a primeira execução.

**Decisão:** MapperExtensions estáticos (`ToCommand()`, `ToResponse()`, `ToEntity()`, `ToDomain()`) compilados junto do código de produção.

**Benefícios:** Erros de mapeamento detectados em tempo de compilação; zero reflexão em runtime; código legível sem magic strings; consistência com a API de OS.

---

## ADR-004 — `[EntityName]` canônico para roteamento cross-service

**Contexto:** MassTransit usa `MessageUrn` (baseado em namespace + nome do tipo) para roteamento de mensagens. Se dois serviços declaram o mesmo record em namespaces diferentes, o roteamento não funciona.

**Decisão:** Todos os contratos cross-service são decorados com `[EntityName("kebab-case")]`. O roteamento passa a depender do `EntityName`, não do namespace .NET. Cada serviço redeclara o record localmente com o mesmo `EntityName` — sem package compartilhado.

**Exemplo:**

```csharp
[EntityName("orcamento.aprovado-pelo-cliente.v1")]
public sealed record OrcamentoAprovadoPeloClienteEvent(Guid OrdemDeServicoId, DateTime AprovadoEm);
```

**Consequências:** Simétrico entre publicador e consumidor; tolerante a refactors de namespace; elimina dependência de pacote compartilhado (ADR futuro quando contratos estabilizarem).

---

## ADR-005 — Webhook HMAC-SHA256 para aprovação assíncrona de orçamento

**Contexto:** O cliente recebe um orçamento por e-mail e precisa aprovar ou rejeitar. As alternativas eram: (a) link para um portal autenticado, (b) link assinado HMAC sem autenticação.

**Decisão:** Link assinado com HMAC-SHA256. O e-mail contém dois links — um para aprovar e um para rejeitar — com parâmetros `token` (assinatura) e `ts` (timestamp Unix).

**Por quê:** Elimina a necessidade de o cliente ter conta no sistema; zero atrito para o fluxo de aprovação; janela de validade configurável (padrão 30 dias); idempotência via tabela `webhook_events`.

**Riscos mitigados:** Falsificação de links (HMAC garante autenticidade); replay attacks (janela de tempo); double-submit (unique constraint em `webhook_events`).

---

## ADR-006 — Escopo `servico-scope` para acesso M2M da API de Pagamentos

**Contexto:** A API de Pagamentos precisa consultar `GET /api/clientes/{id}` para obter e-mail e nome do cliente ao gerar um pagamento. Dar `admin-scope` ao serviço de pagamentos violaria o princípio do menor privilégio.

**Decisão:** Criar um escopo dedicado `servico-scope` (configurável via `AUTH__SERVICO_SCOPE`) que libera apenas a leitura de clientes individuais. O endpoint `GET /api/clientes/{id}` aceita `admin-scope` OU `servico-scope`.

**Consequências:** Pagamentos usa token com `servico-scope` para chamar Cadastros; lógica de autorização isolada em `AuthPolicies.AllowServicoOrAdminScopePolicyName`; outros endpoints sensíveis continuam protegidos por `admin-scope` apenas.

---

## ADR-007 — Outbox transacional para integration events do fluxo webhook

**Contexto:** O webhook de aprovação/rejeição de orçamento é um ponto crítico cross-service: depois que o cliente clica e o webhook commita o registro de idempotência (`webhook_events`), a API de OS precisa receber o evento `OrcamentoAprovadoPeloClienteEvent` ou `OrcamentoRejeitadoPeloClienteEvent` para promover a OS. Antes desta decisão, a publicação era direta via `IPublishEndpoint.Publish` após o commit — se o RabbitMQ estivesse indisponível entre o commit e o publish, o evento se perdia: a próxima tentativa do cliente caía no caminho de duplicata e a OS nunca avançava.

**Decisão:** Outbox transacional na API de Cadastros, com escopo limitado aos integration events do fluxo webhook. Implementação (`Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Outbox`):

- Tabela `OutboxMessages` com índice parcial `WHERE Status = 0` (Pending).
- `IWebhookEventRepository.TryRegisterAsync` recebe o integration event como parâmetro e enfileira a `OutboxMessageEntity` no **mesmo `SaveChanges`** do `webhook_events` — atomicidade garantida pelo PostgreSQL.
- `OutboxProcessor` (BackgroundService): polling de 2s (configurável via `OUTBOX__POLL_INTERVAL`), `SELECT … FOR UPDATE SKIP LOCKED` para multi-instância, dispatch via `IPublishEndpoint`.
- DLQ após `OUTBOX__MAX_RETRIES` (default 10) tentativas.
- `OutboxCleanupService`: remove mensagens `Processed` mais antigas que `OUTBOX__PROCESSED_RETENTION` (default 7 dias).
- Métricas OTel: `outbox.messages.dispatched/failed/dead_lettered` no meter `MecanicaHermes.Cadastros.Outbox`.
- Tracing: `traceparent` capturado no commit e propagado no Activity do dispatch.

**Consequências:** Substitui a decisão original (publicação direta após commit) e fecha o gap descrito em ADR-002 para o caso específico do webhook. As rotas `/api/webhooks/orcamentos/{id}/{aprovar|rejeitar}` deixam de injetar `IPublishEndpoint` — passam o integration event ao `WebhookEventRepository` e o broker é desacoplado do request HTTP do cliente.

**Escopo intencionalmente limitado:** Domain events in-process (`ClienteCriadoEvent` etc.) continuam fora do Outbox (ADR-002). Adoção do padrão para outros casos depende de evidência de que o evento perdido tem efeito colateral cross-service crítico.

**Diferença para o Outbox da OS:** OS dispatcha **domain events** via `IDomainEventDispatcher` (MediatR); Cadastros dispatcha **integration events** direto via `IPublishEndpoint` (MassTransit). A Entity, Configuration, OutboxOptions, Cleanup e Metrics são iguais ao padrão; apenas o `OutboxProcessor` difere no dispatcher injetado.

**Constraint conhecida — resolução de tipo via `AssemblyQualifiedName`:** O processador resolve o `System.Type` da mensagem via `Type.GetType(typeName, throwOnError: false)`, e o `typeName` gravado é o `AssemblyQualifiedName` do record (ex.: `Mecanica.Hermes.Contracts.Integration.OrcamentoAprovadoPeloClienteEvent, Mecanica.Hermes.Contracts, Version=1.0.0.0, …`). Se o assembly do SDK passar de v1 para v2 com **pendências** ainda não despachadas no Outbox, a resolução falha → DLQ depois de `MaxRetries`. Aceito como edge case de janela de deploy: drenar o Outbox antes de cutover MAJOR do SDK, ou implementar fallback de scan por `EntityNameAttribute` quando a mudança ocorrer.

**Cobertura de testes:** Unit tests do `OutboxProcessor` (8 cenários — happy path, tipo inexistente, DLQ, falha de Publish, retry) usam EF InMemory com `AllowNonLockingFallback=true`. Integration test do `WebhookEventRepository.TryRegisterAsync` (atomicidade do `SaveChanges` + rollback no caminho de duplicata) requer Testcontainers Postgres — pendente como follow-up; o fluxo é `db.SaveChanges()` único do EF, comportamento bem-conhecido, mas o catch de `PostgresException 23505` não está coberto.
