using System.Diagnostics;
using Mecanica.Hermes.Shared.Core.Results;
using MediatR;

namespace Mecanica.Hermes.Cadastros.Application.Common.Behaviors;

public sealed class TracingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private static readonly ActivitySource Source =
        new("MecanicaHermes.Cadastros.Application",
            typeof(TracingBehavior<,>).Assembly.GetName().Version?.ToString() ?? "1.0");

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        using var activity = Source.StartActivity(requestName);

        activity?.SetTag("mediator.request", requestName);

        var response = await next(cancellationToken);

        activity?.SetStatus(response.IsSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error);

        return response;
    }
}
