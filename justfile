# Receitas de desenvolvimento local. Requer 'just' (https://github.com/casey/just).
# Roda em PowerShell ou bash. Comando default: lista as receitas.
set windows-shell := ["pwsh.exe", "-NoLogo", "-NoProfile", "-Command"]

default:
    @just --list

# --- Build & test --------------------------------------------------------

# Build da solução em Debug.
build:
    dotnet build Mecanica.Hermes.Cadastros.slnx --nologo

# Roda todos os testes (unit + integration + architecture) com cobertura.
test:
    dotnet test Mecanica.Hermes.Cadastros.slnx --nologo --settings coverlet.runsettings --results-directory .temp/coverage

# Roda apenas testes de unidade (sem Testcontainers).
test-unit:
    dotnet test Mecanica.Hermes.Cadastros.slnx --nologo --filter "FullyQualifiedName!~IntegrationTests&FullyQualifiedName!~Infrastructure.Tests"

# Gera relatório HTML de cobertura em .temp/coverage/report (requer dotnet-reportgenerator-globaltool).
coverage-report: test
    reportgenerator -reports:".temp/coverage/**/coverage.opencover.xml" -targetdir:".temp/coverage/report" -reporttypes:"Html;TextSummary"
    @echo "Relatório: .temp/coverage/report/index.html"

# --- Formatação ----------------------------------------------------------

# Aplica formatação (whitespace + style + analyzers) na solução.
format:
    dotnet format Mecanica.Hermes.Cadastros.slnx

# Verifica formatação sem alterar arquivos. Falha se algo estiver fora do padrão.
# Espelha exatamente as checagens do workflow .github/workflows/format-ci.yml.
format-check:
    dotnet format whitespace Mecanica.Hermes.Cadastros.slnx --verify-no-changes --verbosity diagnostic
    dotnet format style Mecanica.Hermes.Cadastros.slnx --verify-no-changes --verbosity diagnostic
    dotnet format analyzers Mecanica.Hermes.Cadastros.slnx --verify-no-changes --verbosity diagnostic
    @just _check-line-length

# Verifica que nenhum .cs excede 120 colunas (max_line_length do .editorconfig).
_check-line-length:
    #!pwsh
    $max = 120
    $violations = Get-ChildItem -Path src,tests -Recurse -Filter *.cs |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj|Migrations)[\\/]' } |
        ForEach-Object {
            $file = $_.FullName
            $n = 0
            foreach ($line in [System.IO.File]::ReadLines($file)) {
                $n++
                if ($line.Length -gt $max) {
                    "{0}:{1}: {2} cols" -f $file, $n, $line.Length
                }
            }
        }
    if ($violations) {
        $violations | ForEach-Object { Write-Host $_ }
        Write-Host ""
        Write-Error ("{0} linha(s) excedem {1} colunas (max_line_length do .editorconfig)." -f $violations.Count, $max)
        exit 1
    }
    Write-Host ("OK: nenhuma linha excede {0} colunas." -f $max)

# --- Migrations ----------------------------------------------------------

# Cria nova migration (uso: just migration-add NomeDaMigracao).
migration-add name:
    dotnet ef migrations add {{name}} --project src/Mecanica.Hermes.Cadastros.Infrastructure --startup-project src/Mecanica.Hermes.Cadastros.Api

# Aplica migrations no banco apontado pela env var ConnectionStrings__DefaultConnection.
migration-apply:
    dotnet ef database update --project src/Mecanica.Hermes.Cadastros.Infrastructure --startup-project src/Mecanica.Hermes.Cadastros.Api

# Lista migrations conhecidas.
migration-list:
    dotnet ef migrations list --project src/Mecanica.Hermes.Cadastros.Infrastructure --startup-project src/Mecanica.Hermes.Cadastros.Api

# --- Run -----------------------------------------------------------------

# Sobe a API localmente (Development). Requer Postgres + RabbitMQ rodando.
run:
    dotnet run --project src/Mecanica.Hermes.Cadastros.Api

# --- Limpeza -------------------------------------------------------------

# Remove artefatos de build (bin, obj, TestResults, .temp/coverage).
clean:
    Get-ChildItem -Path src,tests -Recurse -Directory -Include bin,obj,TestResults | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Recurse -Force .temp/coverage -ErrorAction SilentlyContinue
