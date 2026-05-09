# Referência de API

> **Documentação interativa**: acesse `/scalar/v1` quando a aplicação estiver rodando para explorar os endpoints com exemplos de request/response em tempo real.

## Autenticação

Todos os endpoints (exceto webhooks e health) exigem **JWT Bearer**. O token deve ser enviado no header:

```text
Authorization: Bearer <token>
```

### Escopos

| Escopo | Quem usa | Permissões |
| --- | --- | --- |
| `cliente-scope` | Usuários finais | Leitura de clientes, veículos, produtos e serviços |
| `admin-scope` | Operadores internos | Leitura + mutação de todos os recursos |
| `servico-scope` | API de Pagamentos (M2M) | `GET /api/clientes/{id}` — consulta de dados do cliente para gerar pagamento |

> Em ambiente `Development`/`Testing`, o `DevelopmentAuthenticationMiddleware` aceita qualquer requisição sem token. Veja [`configuration.md`](./configuration.md#autenticação-jwt).

## Clientes

| Método | Rota | Escopo mínimo | Descrição |
| --- | --- | --- | --- |
| `GET` | `/api/clientes` | `cliente-scope` | Listar clientes (paginado) |
| `GET` | `/api/clientes/{id}` | `admin-scope` **ou** `servico-scope` | Buscar cliente por ID |
| `POST` | `/api/clientes` | `admin-scope` | Criar cliente |
| `PUT` | `/api/clientes/{id}` | `admin-scope` | Atualizar cliente |
| `DELETE` | `/api/clientes/{id}` | `admin-scope` | Remover cliente |

> **Nota:** `GET /api/clientes/{id}` aceita `servico-scope` para que a API de Pagamentos possa consultar e-mail e nome do cliente via M2M sem necessitar de `admin-scope`.

## Veículos

| Método | Rota | Escopo mínimo | Descrição |
| --- | --- | --- | --- |
| `GET` | `/api/clientes/{clienteId}/veiculos` | `cliente-scope` | Listar veículos do cliente |
| `POST` | `/api/clientes/{clienteId}/veiculos` | `admin-scope` | Adicionar veículo |
| `DELETE` | `/api/clientes/{clienteId}/veiculos/{veiculoId}` | `admin-scope` | Remover veículo |

## Produtos

| Método | Rota | Escopo mínimo | Descrição |
| --- | --- | --- | --- |
| `GET` | `/api/produtos` | `cliente-scope` | Listar produtos (paginado) |
| `GET` | `/api/produtos/{id}` | `cliente-scope` | Buscar produto por ID |
| `POST` | `/api/produtos` | `admin-scope` | Criar produto |
| `PUT` | `/api/produtos/{id}` | `admin-scope` | Atualizar produto |
| `DELETE` | `/api/produtos/{id}` | `admin-scope` | Remover produto |

## Serviços

| Método | Rota | Escopo mínimo | Descrição |
| --- | --- | --- | --- |
| `GET` | `/api/servicos` | `cliente-scope` | Listar serviços (paginado) |
| `GET` | `/api/servicos/{id}` | `cliente-scope` | Buscar serviço por ID |
| `POST` | `/api/servicos` | `admin-scope` | Criar serviço |
| `PUT` | `/api/servicos/{id}` | `admin-scope` | Atualizar serviço |
| `DELETE` | `/api/servicos/{id}` | `admin-scope` | Remover serviço |

## Notificações (uso interno)

Esses endpoints são chamados diretamente pela API de OS ou de Pagamentos, ou por admins, para disparar envio de e-mail. **Na integração cross-service normal, o disparo ocorre via eventos MassTransit — não por esses endpoints.** Eles existem para triggers manuais.

| Método | Rota | Escopo | Descrição |
| --- | --- | --- | --- |
| `POST` | `/api/notificacoes/orcamento/{ordemServicoId}/enviar-aprovacao` | `admin-scope` | Envia e-mail de aprovação de orçamento ao cliente |
| `POST` | `/api/notificacoes/pagamento/{pagamentoId}/enviar-link` | `admin-scope` | Envia e-mail com link de pagamento ao cliente |

## Webhooks (públicos — sem autenticação)

Os links de aprovação/rejeição são enviados por e-mail ao cliente. São URLs assinadas com **HMAC-SHA256** via parâmetros `token` e `ts`. Veja [`messaging.md#webhook-hmac`](./messaging.md#webhook-hmac) para detalhes do mecanismo.

| Método | Rota | Parâmetros | Descrição |
| --- | --- | --- | --- |
| `GET` | `/api/webhooks/orcamentos/{id}/aprovar` | `token`, `ts` | Cliente aprova o orçamento. Publica `OrcamentoAprovadoPeloClienteEvent`. Retorna HTML de confirmação. |
| `GET` | `/api/webhooks/orcamentos/{id}/rejeitar` | `token`, `ts`, `motivo?` | Cliente rejeita o orçamento. Publica `OrcamentoRejeitadoPeloClienteEvent`. Retorna HTML de confirmação. |

**Respostas possíveis:**

| Status | Significado |
| --- | --- |
| `200 OK` | Ação registrada com sucesso (HTML de confirmação) |
| `200 OK` (idempotente) | Ação já havia sido registrada anteriormente |
| `400 Bad Request` | Token inválido ou expirado |

## Health checks

| Rota | Descrição |
| --- | --- |
| `GET /healthz/live` | Liveness — aplicação está de pé |
| `GET /healthz/ready` | Readiness — banco disponível |
| `GET /health` | Combinado (usado por orquestradores) |
