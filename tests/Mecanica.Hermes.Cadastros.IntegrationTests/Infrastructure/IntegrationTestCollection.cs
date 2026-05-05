using Mecanica.Hermes.Cadastros.IntegrationTests.Infrastructure;

namespace Mecanica.Hermes.Cadastros.IntegrationTests;

[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebApplicationFactory>
{
    public const string Name = "Integration Tests";
}
