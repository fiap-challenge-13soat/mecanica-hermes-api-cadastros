# Banco de dados

## Tecnologia

**PostgreSQL 18** com **EF Core 10** (code-first, Npgsql driver). Migrations geradas pelo `dotnet ef` e aplicadas automaticamente no startup (configurável via `RUN_MIGRATIONS_ON_STARTUP`).

O `AppDbContext` é registrado com escopo por requisição; o `EfUnitOfWork` encapsula `SaveChangesAsync` e despacha domain events via `IDomainEventDispatcher` após o commit.

## Tabelas

### `clientes`

| Coluna | Tipo | Notas |
| --- | --- | --- |
| `id` | `uuid` | PK |
| `nome_civil` | `varchar` | Value Object `NomeProprioVo` |
| `nome_social` | `varchar?` | Opcional |
| `identificacao_fiscal` | `varchar` | CPF ou CNPJ, validado pelo `IdentificacaoFiscalVo` |
| `email` | `varchar` | Value Object `EmailVo` |
| `telefone` | `varchar` | Value Object `TelefoneVo` |
| `created_at` | `timestamptz` | Preenchido no insert |
| `updated_at` | `timestamptz?` | Atualizado em cada update |

### `veiculos`

| Coluna | Tipo | Notas |
| --- | --- | --- |
| `id` | `uuid` | PK |
| `cliente_id` | `uuid` | FK → `clientes.id` (CASCADE DELETE) |
| `modelo` | `varchar` | |
| `marca` | `varchar` | |
| `placa` | `varchar` | Value Object `PlacaVo` (formato Mercosul ou antigo) |
| `ano` | `int` | |
| `created_at` | `timestamptz` | |

### `produtos`

| Coluna | Tipo | Notas |
| --- | --- | --- |
| `id` | `uuid` | PK |
| `descricao` | `varchar` | Value Object `DescricaoProdutoVo` |
| `valor` | `numeric(18,2)` | Value Object `ValorProdutoVo` (não negativo) |
| `quantidade` | `int` | Value Object `QuantidadeProdutoVo` (positivo) |
| `tipo` | `varchar` | Ex.: `"Peca"`, `"Fluido"` |
| `created_at` | `timestamptz` | |
| `updated_at` | `timestamptz?` | |

### `servicos`

| Coluna | Tipo | Notas |
| --- | --- | --- |
| `id` | `uuid` | PK |
| `descricao` | `varchar` | Value Object `DescricaoProdutoVo` |
| `valor` | `numeric(18,2)` | Value Object `ValorProdutoVo` |
| `created_at` | `timestamptz` | |
| `updated_at` | `timestamptz?` | |

### `webhook_events`

Tabela de idempotência dos webhooks de aprovação/rejeição de orçamento.

| Coluna | Tipo | Notas |
| --- | --- | --- |
| `id` | `uuid` | PK |
| `ordem_de_servico_id` | `uuid` | Índice único com `acao` |
| `acao` | `varchar` | `"aprovar"` ou `"rejeitar"` |
| `processado_em` | `timestamptz` | |

> **Constraint única:** `(ordem_de_servico_id, acao)` — garante que o mesmo orçamento não seja aprovado/rejeitado duas vezes, mesmo em caso de retry do e-mail.

## Migrations

### Criar nova migration

```bash
just migration-add NomeDaMigration
# ou:
dotnet ef migrations add NomeDaMigration \
  --project src/Mecanica.Hermes.Cadastros.Infrastructure \
  --startup-project src/Mecanica.Hermes.Cadastros.Api
```

### Aplicar migrations

**Automático no startup** (padrão): a API aplica migrations via `MigrateDatabaseAsync` ao iniciar.
Para desabilitar em ambientes onde migrations rodam num pipeline separado, configure
`RUN_MIGRATIONS_ON_STARTUP=false`.

**Manual via CLI:**
```bash
just migration-apply
# ou:
dotnet ef database update \
  --project src/Mecanica.Hermes.Cadastros.Infrastructure \
  --startup-project src/Mecanica.Hermes.Cadastros.Api
```

### Listar migrations

```bash
just migration-list
```

### Reverter uma migration

```bash
dotnet ef database update <NomeMigrationAnterior> \
  --project src/Mecanica.Hermes.Cadastros.Infrastructure \
  --startup-project src/Mecanica.Hermes.Cadastros.Api
```

### Convenção de nome

Padrão `{Verbo}{Entidade}` em PascalCase. Exemplos:

- `InitialCreate`
- `AddWebhookEvents`
- `AddOutboxMessages`
- `AlterClienteAddIsDeleted`

**Não** use `Migration1`, `Temp`, `Fix`, `Test`. O nome deve descrever a mudança de schema —
a data já vem no prefixo gerado pelo EF.
