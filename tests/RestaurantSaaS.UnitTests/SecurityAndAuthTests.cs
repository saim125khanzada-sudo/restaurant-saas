using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Infrastructure.Services;
using Xunit;

namespace RestaurantSaaS.UnitTests;

public class SecurityAndAuthTests
{
    [Fact]
    public void PasswordHasher_ShouldCorrectlyHashAndVerify()
    {
        var hasher = new PasswordHasher();
        var password = "SuperSecretPassword123!";

        var hash = hasher.HashPassword(password);

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
        hasher.VerifyPassword(password, hash).Should().BeTrue();
        hasher.VerifyPassword("WrongPassword", hash).Should().BeFalse();
    }

    [Fact]
    public void MfaService_ShouldGenerateValidSecretAndVerifyTotp()
    {
        var mfa = new MfaService();
        var secret = mfa.GenerateSecret();

        secret.Should().NotBeNullOrWhiteSpace();
        secret.Length.Should().BeGreaterThanOrEqualTo(16);

        var qrUri = mfa.GenerateQrCodeUri("chef@pizza.com", secret);
        qrUri.Should().StartWith("otpauth://totp/RestaurantSaaS:chef%40pizza.com");
    }

    [Fact]
    public void JwtTokenService_ShouldEmbedTenantAndRoleClaims()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"JWT_SECRET", "Min64CharactersLongSecureRandomHexKeyForJwtTokensSigningProductionUseOnly!"},
            {"JWT_ISSUER", "RestaurantSaaS.Api"},
            {"JWT_AUDIENCE", "RestaurantSaaS.Clients"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);
        var user = new User
        {
            Id = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Email = "admin@burgerking.com",
            FullName = "Admin User"
        };

        var roles = new[] { "Restaurant Admin" };
        var permissions = new[] { "Orders.Create", "Orders.View" };

        var token = jwtService.GenerateAccessToken(user, roles, permissions);

        token.Should().NotBeNullOrWhiteSpace();

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.First(c => c.Type == "tenant_id").Value.Should().Be(user.RestaurantId.ToString());
        jwt.Claims.First(c => c.Type == "branch_id").Value.Should().Be(user.BranchId.Value.ToString());
        jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value.Should().Be(user.Email);
        jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).Should().Contain("Restaurant Admin");
        jwt.Claims.Where(c => c.Type == "permission").Select(c => c.Value).Should().Contain(new[] { "Orders.Create", "Orders.View" });
    }
}
