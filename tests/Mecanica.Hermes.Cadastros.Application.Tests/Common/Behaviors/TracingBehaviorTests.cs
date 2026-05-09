using System.Diagnostics;
using Mecanica.Hermes.Cadastros.Application.Common.Behaviors;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Tests.Common.Behaviors;

public sealed class TracingBehaviorTests : IDisposable
{
    private readonly List<Activity> _captured = [];
    private readonly ActivityListener _listener;

    public TracingBehaviorTests()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "MecanicaHermes.Cadastros.Application",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activity => _captured.Add(activity),
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public void Dispose()
    {
        _listener.Dispose();
        GC.SuppressFinalize(this);
    }

    private sealed record FakeRequest : IRequest<Result>;

    [Fact]
    public async Task Handle_ResultadoOk_DefineActivityStatusOk()
    {
        var sut = new TracingBehavior<FakeRequest, Result>();

        var response = await sut.Handle(
            new FakeRequest(),
            _ => Task.FromResult(Result.Ok()),
            CancellationToken.None);

        response.IsSuccess.Should().BeTrue();
        var activity = _captured.Should().ContainSingle().Subject;
        activity.OperationName.Should().Be(nameof(FakeRequest));
        activity.Status.Should().Be(ActivityStatusCode.Ok);
        activity.GetTagItem("mediator.request").Should().Be(nameof(FakeRequest));
    }

    [Fact]
    public async Task Handle_ResultadoFailure_DefineActivityStatusError()
    {
        var sut = new TracingBehavior<FakeRequest, Result>();

        var response = await sut.Handle(
            new FakeRequest(),
            _ => Task.FromResult(Result.BadRequest("erro")),
            CancellationToken.None);

        response.IsSuccess.Should().BeFalse();
        var activity = _captured.Should().ContainSingle().Subject;
        activity.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Fact]
    public async Task Handle_PropagaResultadoDoNext()
    {
        var sut = new TracingBehavior<FakeRequest, Result>();
        var expected = Result.NotFound();

        var response = await sut.Handle(
            new FakeRequest(),
            _ => Task.FromResult(expected),
            CancellationToken.None);

        response.Should().BeSameAs(expected);
    }
}
