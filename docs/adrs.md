# Decisões de Arquitetura (ADRs)

## ADR-001 — Sem SAGA

**Contexto:** A API de OS usa uma SAGA MassTransit para serializar operações por `OrdemDeServicoId`. Precisávamos decidir se Cadastros seguiria o mesmo padrão.

**Decisão:** Sem SAGA. O fluxo de Cadastros é linear: evento MassTransit recebido → command MediatR → handler → e-mail. Não há operações concorrentes por entidade que precisem de serialização.

**Consequências:** Código mais simples; sem dependência de MongoDB para estado de SAGA; sem timeout sentinel. Adequado para este perfil de uso.

---

## ADR-002 — Sem Outbox transacional

**Contexto:** A API de OS persiste domain events no Outbox na mesma transação do aggregate, garantindo at-least-once delivery. Cadastros tem domain events (ex.: `ClienteCriadoEvent`) que incrementam métricas OTel.

**Decisão:** Sem Outbox. Os domain events são despachados diretamente pelo `IDomainEventDispatcher` após o `CommitAsync`. Se a aplicação cair entre o commit e o dispatch, os eventos são perdidos.

**Por quê aceitamos:** As métricas OTel são best-effort — perder um counter ocasionalmente é aceitável. Implementar o Outbox adicionaria uma tabela extra e um background service sem benefício proporcional.

**Revisão:** Se Cadastros passar a ter domain events com efeitos colaterais críticos (ex.: cobranças, notificações obrigatórias), reconsiderar.

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
