using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Repositories;

internal static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IServicoRepository, ServicoRepository>();
        return services;
    }
}
