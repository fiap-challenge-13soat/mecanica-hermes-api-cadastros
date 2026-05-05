namespace Mecanica.Hermes.Cadastros.Infrastructure.Settings;

public static class AuthPolicies
{
    public static readonly string AllowClienteScope =
        Environment.GetEnvironmentVariable(VariableNames.AuthClienteScope) ?? "cliente-scope";

    public static readonly string OnlyAdminScope =
        Environment.GetEnvironmentVariable(VariableNames.AuthAdminScope) ?? "admin-scope";

    public static readonly string AllowServicoScope =
        Environment.GetEnvironmentVariable(VariableNames.AuthServicoScope) ?? "servico-scope";

    public const string AllowServicoOrAdminScopePolicyName = "AllowServicoOrAdminScope";
}
