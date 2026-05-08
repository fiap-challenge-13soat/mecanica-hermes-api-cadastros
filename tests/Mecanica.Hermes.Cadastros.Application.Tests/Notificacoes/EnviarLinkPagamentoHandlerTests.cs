using MassTransit;
using Mecanica.Hermes.Cadastros.Application.Common.Messaging;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarLinkPagamento;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;

namespace Mecanica.Hermes.Cadastros.Application.Tests.Notificacoes;

public class EnviarLinkPagamentoHandlerTests
{
    private readonly Mock<IPagamentoApiClient> _pagamentoClient = new();
    private readonly Mock<IPublishEndpoint> _publish = new();

    private EnviarLinkPagamentoCommandHandler CreateHandler() =>
        new(_pagamentoClient.Object, _publish.Object);

    private static PagamentoExternalDto NovoPagamento(
        PagamentoLinkExternalDto? link = null, string email = "cliente@x.com") =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            email, "Cliente Teste", 99m, "BRL", "AguardandoPagamento",
            link, DateTime.UtcNow.AddDays(1));

    [Fact]
    public async Task Handle_PagamentoNaoEncontrado_RetornaNotFound()
    {
        _pagamentoClient.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagamentoExternalDto?)null);

        var result = await CreateHandler().Handle(new EnviarLinkPagamentoCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SemLink_RetornaBadRequest()
    {
        _pagamentoClient.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NovoPagamento(link: null));

        var result = await CreateHandler().Handle(new EnviarLinkPagamentoCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_LinkVazio_RetornaBadRequest()
    {
        _pagamentoClient.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NovoPagamento(new PagamentoLinkExternalDto("p", "  ")));

        var result = await CreateHandler().Handle(new EnviarLinkPagamentoCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmailVazio_RetornaBadRequest()
    {
        _pagamentoClient.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NovoPagamento(new PagamentoLinkExternalDto("p", "https://mp/x"), email: ""));

        var result = await CreateHandler().Handle(new EnviarLinkPagamentoCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DadosCompletos_PublicaEAceita()
    {
        _pagamentoClient.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NovoPagamento(new PagamentoLinkExternalDto("p", "https://mp/x")));

        var result = await CreateHandler().Handle(new EnviarLinkPagamentoCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _publish.Verify(p => p.Publish(It.IsAny<EnviarEmailCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
