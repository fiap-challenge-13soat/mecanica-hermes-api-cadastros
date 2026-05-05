using Mecanica.Hermes.Cadastros.Domain.Clientes;
using Mecanica.Hermes.Cadastros.Domain.Produtos;

namespace Mecanica.Hermes.Cadastros.Application.Tests.Common;

internal static class TestData
{
    public const string Cpf = "52998224725";
    public const string Cnpj = "11444777000161";
    public const string Email = "joao.silva@empresa.com";
    public const string Telefone = "11987654321";
    public const string NomeCivil = "João Silva";
    public const string Placa = "ABC1D23";

    public static Cliente NovoCliente(string email = Email, string cpf = Cpf) =>
        Cliente.Criar(NomeCivil, null, cpf, email, Telefone).Data!;

    public static Cliente NovoClienteComVeiculo()
    {
        var cliente = NovoCliente();
        cliente.AdicionarVeiculo("Onix", "Chevrolet", Placa, 2022);
        cliente.ClearDomainEvents();
        return cliente;
    }

    public static Produto NovoProduto() =>
        Produto.Criar("Filtro de Oleo", 50m, 10, "Peca").Data!;

    public static Servico NovoServico() =>
        Servico.Criar("Troca de Oleo", 120m).Data!;
}
