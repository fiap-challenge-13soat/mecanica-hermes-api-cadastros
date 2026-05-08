using Mecanica.Hermes.Cadastros.Application.Notificacoes.Ports;
using Mecanica.Hermes.Cadastros.Infrastructure.Notificacoes;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Tests.Notificacoes;

[Collection("WebhookSecret")]
public class NotificacoesUrlBuilderTests : IDisposable
{
    public NotificacoesUrlBuilderTests()
    {
        Environment.SetEnvironmentVariable("APP__BASE_URL", "https://app.test/");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("APP__BASE_URL", null);
    }

    [Fact]
    public void BuildAprovacaoUrl_IncluiTokenETs()
    {
        var signer = new Mock<IWebhookTokenSigner>();
        signer.Setup(s => s.Sign(It.IsAny<Guid>(), "aprovar")).Returns(("hash-abc", 12345L));
        var builder = new NotificacoesUrlBuilder(signer.Object);
        var id = Guid.NewGuid();

        var url = builder.BuildAprovacaoUrl(id);

        url.Should().Contain("https://app.test/api/webhooks/orcamentos/")
            .And.Contain("/aprovar")
            .And.Contain("token=hash-abc")
            .And.Contain("ts=12345");
    }

    [Fact]
    public void BuildRejeicaoUrl_IncluiTokenETs()
    {
        var signer = new Mock<IWebhookTokenSigner>();
        signer.Setup(s => s.Sign(It.IsAny<Guid>(), "rejeitar")).Returns(("hash-def", 67890L));
        var builder = new NotificacoesUrlBuilder(signer.Object);

        var url = builder.BuildRejeicaoUrl(Guid.NewGuid());

        url.Should().Contain("/rejeitar")
            .And.Contain("token=hash-def")
            .And.Contain("ts=67890");
    }

    [Fact]
    public void BaseUrlAusente_UsaLocalhostDefault()
    {
        Environment.SetEnvironmentVariable("APP__BASE_URL", null);
        var signer = new Mock<IWebhookTokenSigner>();
        signer.Setup(s => s.Sign(It.IsAny<Guid>(), It.IsAny<string>())).Returns(("t", 1L));
        var builder = new NotificacoesUrlBuilder(signer.Object);

        builder.BuildAprovacaoUrl(Guid.NewGuid()).Should().StartWith("http://localhost:5000/");
    }
}
