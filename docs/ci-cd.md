# CI/CD

## Workflows

| Arquivo | Gatilho | Propósito |
| --- | --- | --- |
| `build-and-tests-ci.yml` | Push em `main` e `feature/**` | Build + testes com cobertura ≥ 80% |
| `format-ci.yml` | Push em `main` e PR para `main` | Verificação de formatação `.editorconfig` |
| `docker-ci.yml` | CI verde em `main` ou push de tag `v*.*.*` | Build e push da imagem Docker |
| `sonarqube-ci.yml` | Push em `main` e PRs | Análise estática + cobertura no SonarCloud |
| `k8s-trigger-cd.yml` | Pós `docker-ci.yml` com sucesso | Trigger de deploy no repo `mecanica-hermes-k8s` |

## `build-and-tests-ci.yml`

Etapas:

1. Checkout + setup .NET 10
2. Restaurar pacotes (cache NuGet por hash do `Directory.Packages.props`)
3. `dotnet build Mecanica.Hermes.Cadastros.slnx -c Release`
4. `dotnet test` com `coverlet.runsettings` → gera `coverage.opencover.xml`
5. `reportgenerator` → gera `Summary.json`
6. Verifica cobertura de linhas ≥ 80% com `jq` + `awk`
7. Publica artefato HTML (`cobertura-html`, retenção 7 dias)

## `format-ci.yml`

Verifica:

- `dotnet format whitespace --verify-no-changes`
- `dotnet format style --verify-no-changes`
- `dotnet format analyzers --verify-no-changes`
- Linhas com mais de **120 colunas** (regra do `.editorconfig`)

Em caso de falha, exibe sugestão de correção:
```bash
dotnet format Mecanica.Hermes.Cadastros.slnx
```

## `docker-ci.yml`

- Imagem: `mechermes/mecanica-hermes-cadastros`
- Tags geradas:
  - `sha-<short>` — sempre
  - `latest` — apenas via CI de `main`
  - `X.Y.Z`, `X.Y`, `X` — via push de tag `vX.Y.Z`
- Multi-arch: `linux/amd64` e `linux/arm64` via QEMU + Buildx
- Cache: GitHub Actions cache (`type=gha`)

## `sonarqube-ci.yml`

- Projeto SonarCloud: `fiap-challenge-13soat_mecanica-hermes-cadastros`
- Org: `fiap-challenge-13soat`
- Alimenta cobertura via `**/coverage.opencover.xml`
- Em PRs, decora o PR com análise de bugs, code smells e duplicações

## `k8s-trigger-cd.yml`

Dispara `workflow_dispatch` no repo `mecanica-hermes-k8s`:

| Condição | Ambiente | `image_tag` |
| --- | --- | --- |
| CI de `main` passou | `hml` | `main` |
| CI de tag `vX.Y.Z` passou | `prd` | `X.Y.Z` |

## Secrets necessários

Configure em **Settings → Secrets → Actions** do repositório:

| Secret | Usado por | Descrição |
| --- | --- | --- |
| `DOCKER_USERNAME` | `docker-ci.yml` | Usuário Docker Hub |
| `DOCKER_PASSWORD` | `docker-ci.yml` | Token Docker Hub |
| `SONAR_TOKEN` | `sonarqube-ci.yml` | Token SonarCloud |
| `K8S_REPO_PAT` | `k8s-trigger-cd.yml` | GitHub PAT com `workflow` scope no repo K8s |
