using Mecanica.Hermes.Cadastros.Domain.Clientes;
using Mecanica.Hermes.Cadastros.Domain.Produtos;
using Mecanica.Hermes.Cadastros.Infrastructure.Persistence.Mappings;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Tests.Persistence.Mappings;

public class MappersTests
{
    [Fact]
    public void ClienteMapper_RoundTrip_Domain_Entity_Domain()
    {
        var original = Cliente.Criar("Joao Silva", "Joana Silva", "52998224725", "j@x.com", "11987654321").Data!;
        original.AdicionarVeiculo("Onix", "Chevrolet", "ABC1D23", 2022);

        var entity = original.ToEntity();
        var roundTrip = entity.ToDomain();

        roundTrip.NomeCivil.Valor.Should().Be(original.NomeCivil.Valor);
        roundTrip.NomeSocial!.Valor.Should().Be(original.NomeSocial!.Valor);
        roundTrip.Email.Valor.Should().Be(original.Email.Valor);
        roundTrip.IdentificacaoFiscal.Valor.Should().Be(original.IdentificacaoFiscal.Valor);
        roundTrip.Telefone.Valor.Should().Be(original.Telefone.Valor);
        roundTrip.Veiculos.Should().HaveCount(1);
    }

    [Fact]
    public void VeiculoMapper_ToEntity_PreencheClienteId()
    {
        var cliente = Cliente.Criar("Joao Silva", null, "52998224725", "j@x.com", "11987654321").Data!;
        cliente.AdicionarVeiculo("Onix", "Chevrolet", "ABC1D23", 2022);
        var veiculo = cliente.Veiculos.First();

        var entity = veiculo.ToEntity(cliente);

        entity.ClienteId.Should().Be(cliente.Id);
        entity.Placa.Valor.Should().Be("ABC1D23");
    }

    [Fact]
    public void ProdutoMapper_RoundTrip_Domain_Entity_Domain()
    {
        var original = Produto.Criar("Filtro de Oleo", 50m, 10, "Peca").Data!;

        var entity = original.ToEntity();
        var roundTrip = entity.ToDomain();

        roundTrip.Descricao.Valor.Should().Be(original.Descricao.Valor);
        roundTrip.Valor.Valor.Should().Be(original.Valor.Valor);
        roundTrip.Quantidade.Valor.Should().Be(original.Quantidade.Valor);
        roundTrip.Tipo.Should().Be(original.Tipo);
    }

    [Fact]
    public void ServicoMapper_RoundTrip_Domain_Entity_Domain()
    {
        var original = Servico.Criar("Troca de Oleo", 120m).Data!;

        var entity = original.ToEntity();
        var roundTrip = entity.ToDomain();

        roundTrip.Descricao.Valor.Should().Be(original.Descricao.Valor);
        roundTrip.Valor.Valor.Should().Be(original.Valor.Valor);
    }
}
