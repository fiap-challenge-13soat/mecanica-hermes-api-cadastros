using Mecanica.Hermes.Cadastros.Application;
using Mecanica.Hermes.Cadastros.Domain;
using MediatR;
using NetArchTest.Rules;

#pragma warning disable CA1707
#pragma warning disable CA1724

namespace Mecanica.Hermes.Cadastros.ArchitectureTests;

[Trait("Category", "Unit")]
public class ArchitectureTests
{
    private const string DomainNamespace = "Mecanica.Hermes.Cadastros.Domain";
    private const string ApplicationNamespace = "Mecanica.Hermes.Cadastros.Application";
    private const string InfrastructureNamespace = "Mecanica.Hermes.Cadastros.Infrastructure";
    private const string ApiNamespace = "Mecanica.Hermes.Cadastros.Api";

    private static Types FromDomain => Types.InAssembly(typeof(IDomainAssemblyMarker).Assembly);
    private static Types FromApplication => Types.InAssembly(typeof(IApplicationAssemblyMarker).Assembly);

    [Fact]
    public void Domain_NaoDeveDependerDe_Application()
    {
        var result = FromDomain
            .That().ResideInNamespace(DomainNamespace)
            .ShouldNot().HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should()
            .BeTrue($"Domain não deve depender de Application. Violações: {Violations(result)}");
    }

    [Fact]
    public void Domain_NaoDeveDependerDe_Infrastructure()
    {
        var result = FromDomain
            .That().ResideInNamespace(DomainNamespace)
            .ShouldNot().HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should()
            .BeTrue($"Domain não deve depender de Infrastructure. Violações: {Violations(result)}");
    }

    [Fact]
    public void Domain_NaoDeveDependerDe_Api()
    {
        var result = FromDomain
            .That().ResideInNamespace(DomainNamespace)
            .ShouldNot().HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue($"Domain não deve depender de Api. Violações: {Violations(result)}");
    }

    [Fact]
    public void Application_NaoDeveDependerDe_Infrastructure()
    {
        var result = FromApplication
            .That().ResideInNamespace(ApplicationNamespace)
            .ShouldNot().HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should()
            .BeTrue($"Application não deve depender de Infrastructure. Violações: {Violations(result)}");
    }

    [Fact]
    public void Application_NaoDeveDependerDe_Api()
    {
        var result = FromApplication
            .That().ResideInNamespace(ApplicationNamespace)
            .ShouldNot().HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should()
            .BeTrue($"Application não deve depender de Api. Violações: {Violations(result)}");
    }

    [Fact]
    public void Application_NaoDeveTerDependenciaDiretaDeEntityFramework()
    {
        var result = FromApplication
            .That().ResideInNamespace(ApplicationNamespace)
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should()
            .BeTrue($"Application não deve referenciar EntityFrameworkCore. Violações: {Violations(result)}");
    }

    [Fact]
    public void MediatRHandlers_DevemSerSealed()
    {
        var result = FromApplication
            .That().ImplementInterface(typeof(IRequestHandler<,>))
            .Should().BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue($"Handlers MediatR devem ser sealed. Violações: {Violations(result)}");
    }

    private static string Violations(TestResult result)
    {
        if (result.FailingTypes is null) return "(nenhuma)";
        return string.Join(", ", result.FailingTypes.Select(t => t.FullName ?? t.Name));
    }
}
