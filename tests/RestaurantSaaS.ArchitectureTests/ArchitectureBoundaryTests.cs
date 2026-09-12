using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace RestaurantSaaS.ArchitectureTests;

public class ArchitectureBoundaryTests
{
    [Fact]
    public void Domain_ShouldNotHaveDependencyOnOtherProjects()
    {
        var result = Types.InAssembly(typeof(RestaurantSaaS.Domain.Entities.Restaurant).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "RestaurantSaaS.Application",
                "RestaurantSaaS.Infrastructure",
                "RestaurantSaaS.Api"
            )
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Domain layer must be completely isolated from external dependencies.");
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(typeof(RestaurantSaaS.Application.Interfaces.IApplicationDbContext).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "RestaurantSaaS.Infrastructure",
                "RestaurantSaaS.Api"
            )
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Application layer must not depend on Infrastructure or Presentation.");
    }
}
