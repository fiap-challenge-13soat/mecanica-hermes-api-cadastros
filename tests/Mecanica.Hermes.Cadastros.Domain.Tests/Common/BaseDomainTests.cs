using Mecanica.Hermes.Cadastros.Domain.Clientes;
using Mecanica.Hermes.Cadastros.Domain.Common.Clock;

namespace Mecanica.Hermes.Cadastros.Domain.Tests.Common;

public class BaseDomainTests
{
    [Fact]
    public void NovoAggregate_PreencheCreatedAtComUtcNow()
    {
        var antes = DomainClock.UtcNow.AddSeconds(-1);

        var cliente = Cliente.Criar("João Silva", null, "52998224725", "j@x.com", "11987654321").Data!;

        cliente.CreatedAt.Should().BeAfter(antes);
        cliente.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void DomainClock_RetornaUtc()
    {
        DomainClock.UtcNow.Kind.Should().Be(DateTimeKind.Utc);
    }
}
