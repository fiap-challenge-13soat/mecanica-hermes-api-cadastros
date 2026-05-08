using MassTransit;
using Mecanica.Hermes.Cadastros.Application.Clientes.Ports;
using Mecanica.Hermes.Cadastros.Application.Common.Messaging;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Commands.EnviarOrcamentoParaAprovacao;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Dtos;
using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Mecanica.Hermes.Cadastros.Application.Produtos.Ports;
using Mecanica.Hermes.Cadastros.Application.Tests.Common;
using Mecanica.Hermes.Cadastros.Domain.Clientes;

namespace Mecanica.Hermes.Cadastros.Application.Tests.Notificacoes;

public class EnviarOrcamentoParaAprovacaoHandlerTests
{
    private readonly Mock<IOrdemServicoApiClient> _osClient = new();
    private readonly Mock<IClienteRepository> _clienteRepo = new();
    private readonly Mock<IProdutoRepository> _produtoRepo = new();
    private readonly Mock<IServicoRepository> _servicoRepo = new();
    private readonly Mock<INotificacoesUrlBuilder> _urlBuilder = new();
    private readonly Mock<IPublishEndpoint> _publish = new();

    private EnviarOrcamentoParaAprovacaoCommandHandler CreateHandler()
    {
        _urlBuilder.Setup(u => u.BuildAprovacaoUrl(It.IsAny<Guid>())).Returns("https://app/aprovar");
        _urlBuilder.Setup(u => u.BuildRejeicaoUrl(It.IsAny<Guid>())).Returns("https://app/rejeitar");
        return new EnviarOrcamentoParaAprovacaoCommandHandler(
            _osClient.Object, _clienteRepo.Object, _produtoRepo.Object, _servicoRepo.Object,
            _urlBuilder.Object, _publish.Object);
    }

    private static OrdemDeServicoExternalDto NovaOrdem(string status, Guid? clienteId = null) =>
        new(Guid.NewGuid(), clienteId ?? Guid.NewGuid(), Guid.NewGuid(),
            "Problema", "Obs", status, 100m,
            [new OrdemDeServicoProdutoExternalDto(Guid.NewGuid(), 50m, 1)],
            [new OrdemDeServicoServicoExternalDto(Guid.NewGuid(), 50m)]);

    [Fact]
    public async Task Handle_OsNaoEncontrada_RetornaNotFound()
    {
        _osClient.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemDeServicoExternalDto?)null);

        var result = await CreateHandler().Handle(
            new EnviarOrcamentoParaAprovacaoCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_StatusNaoAguardandoAprovacao_RetornaBadRequest()
    {
        _osClient.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NovaOrdem("EmDiagnostico"));

        var result = await CreateHandler().Handle(
            new EnviarOrcamentoParaAprovacaoCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ClienteNaoEncontrado_RetornaBadRequest()
    {
        _osClient.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NovaOrdem("AguardandoAprovacao"));
        _clienteRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);

        var result = await CreateHandler().Handle(
            new EnviarOrcamentoParaAprovacaoCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ClienteEncontrado_PublicaEnviarEmailERetornaAccepted()
    {
        var ordem = NovaOrdem("AguardandoAprovacao");
        _osClient.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(ordem);
        _clienteRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(TestData.NovoCliente());
        _produtoRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(TestData.NovoProduto());
        _servicoRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(TestData.NovoServico());

        var result = await CreateHandler().Handle(
            new EnviarOrcamentoParaAprovacaoCommand(ordem.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _publish.Verify(p => p.Publish(It.IsAny<EnviarEmailCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProdutoEServicoNaoEncontradosLocal_AindaEnvia()
    {
        var ordem = NovaOrdem("AguardandoAprovacao");
        _osClient.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(ordem);
        _clienteRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(TestData.NovoCliente());
        // produto e servico ficam null

        var result = await CreateHandler().Handle(
            new EnviarOrcamentoParaAprovacaoCommand(ordem.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
