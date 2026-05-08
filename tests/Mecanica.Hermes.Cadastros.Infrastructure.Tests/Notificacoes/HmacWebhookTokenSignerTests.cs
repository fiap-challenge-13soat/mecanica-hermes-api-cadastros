using Mecanica.Hermes.Cadastros.Infrastructure.Notificacoes;

namespace Mecanica.Hermes.Cadastros.Infrastructure.Tests.Notificacoes;

[Collection("WebhookSecret")]
public class HmacWebhookTokenSignerTests : IDisposable
{
    public HmacWebhookTokenSignerTests()
    {
        Environment.SetEnvironmentVariable("WEBHOOK__SIGNING_SECRET", "test-secret-test-secret-test-secret");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("WEBHOOK__SIGNING_SECRET", null);
    }

    [Fact]
    public void Sign_RetornaTokenETimestamp()
    {
        var signer = new HmacWebhookTokenSigner();

        var (token, ts) = signer.Sign(Guid.NewGuid(), "aprovar");

        token.Should().NotBeNullOrEmpty();
        ts.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Validate_TokenAssinado_RetornaTrue()
    {
        var signer = new HmacWebhookTokenSigner();
        var id = Guid.NewGuid();
        var (token, ts) = signer.Sign(id, "aprovar");

        signer.Validate(id, "aprovar", token, ts).Should().BeTrue();
    }

    [Fact]
    public void Validate_AcaoDiferente_RetornaFalse()
    {
        var signer = new HmacWebhookTokenSigner();
        var id = Guid.NewGuid();
        var (token, ts) = signer.Sign(id, "aprovar");

        signer.Validate(id, "rejeitar", token, ts).Should().BeFalse();
    }

    [Fact]
    public void Validate_IdDiferente_RetornaFalse()
    {
        var signer = new HmacWebhookTokenSigner();
        var (token, ts) = signer.Sign(Guid.NewGuid(), "aprovar");

        signer.Validate(Guid.NewGuid(), "aprovar", token, ts).Should().BeFalse();
    }

    [Fact]
    public void Validate_TokenVazio_RetornaFalse()
    {
        var signer = new HmacWebhookTokenSigner();

        signer.Validate(Guid.NewGuid(), "aprovar", "", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            .Should().BeFalse();
    }

    [Fact]
    public void Validate_TimestampMuitoVelho_RetornaFalse()
    {
        var signer = new HmacWebhookTokenSigner();
        var id = Guid.NewGuid();
        var oldTs = DateTimeOffset.UtcNow.AddDays(-365).ToUnixTimeSeconds();
        var (token, _) = signer.Sign(id, "aprovar");

        signer.Validate(id, "aprovar", token, oldTs).Should().BeFalse();
    }

    [Fact]
    public void Validate_TimestampNoFuturo_RetornaFalse()
    {
        var signer = new HmacWebhookTokenSigner();
        var id = Guid.NewGuid();
        var futuroTs = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();

        signer.Validate(id, "aprovar", "qualquertoken", futuroTs).Should().BeFalse();
    }

    [Fact]
    public void Construtor_SegredoAusente_LancaInvalidOperation()
    {
        Environment.SetEnvironmentVariable("WEBHOOK__SIGNING_SECRET", null);

        Action act = () => new HmacWebhookTokenSigner();

        act.Should().Throw<InvalidOperationException>();
    }
}
