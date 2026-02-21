using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeliverySaaS.Application.Integration;
using DeliverySaaS.Application.Orders;
using DeliverySaaS.Domain.Operations.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DeliverySaaS.Tests.Integration;

public class TestApiFactory : WebApplicationFactory<Program>
{
    public FakeOrderService OrderService { get; } = new();
    public FakeOrderMobileService OrderMobileService { get; } = new();
    public FakeOrderProblemService OrderProblemService { get; } = new();
    public FakeIntegrationService IntegrationService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=DeliverySaaSTests;Trusted_Connection=True;",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:SecretKey"] = "THIS_IS_A_TEST_SECRET_KEY_1234567890"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IOrderService>(OrderService);
            services.AddSingleton<IOrderMobileService>(OrderMobileService);
            services.AddSingleton<IOrderProblemService>(OrderProblemService);
            services.AddSingleton<IIntegrationService>(IntegrationService);
        });
    }

    public static string CreateJwt(params Claim[] claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("THIS_IS_A_TEST_SECRET_KEY_1234567890"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class FakeOrderService : IOrderService
{
    public List<(Guid OrderId, OperationalState ToState)> Calls { get; } = new();

    public Task TransitionAsync(Guid orderId, OperationalState toState, CancellationToken cancellationToken = default)
    {
        Calls.Add((orderId, toState));
        return Task.CompletedTask;
    }
}

public class FakeOrderMobileService : IOrderMobileService
{
    public Task<MerchantDashboardDto> GetMerchantDashboardAsync(Guid merchantId, CancellationToken cancellationToken = default)
        => Task.FromResult(new MerchantDashboardDto(5, 1));

    public Task<Guid> CreateOrderByReservedQrAsync(CreateOrderByReservedQrRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Guid.NewGuid());

    public Task<IReadOnlyList<PickupTaskDto>> GetPickupTasksAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        => Task.FromResult(new List<PickupTaskDto>
        {
            new(Guid.NewGuid(), "ORD-T", "C", "010", "Addr", OperationalState.New)
        });
}

public class FakeOrderProblemService : IOrderProblemService
{
    public Task<Guid> CreateProblemAsync(Guid orderId, Guid problemCatalogId, string? notes, CancellationToken cancellationToken = default)
        => Task.FromResult(Guid.NewGuid());

    public Task ResolveProblemAsync(Guid problemId, DynamicResolutionType resolutionType, string? phone, decimal? amountToCollect, string? address, string? note, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

public class FakeIntegrationService : IIntegrationService
{
    public Task<Guid> CreatePartnerConnectionAsync(string partnerName, string baseUrl, string apiKey, CancellationToken cancellationToken = default)
        => Task.FromResult(Guid.NewGuid());

    public Task<Guid> CreateOutboundHandoffAsync(Guid orderId, Guid governorateId, CancellationToken cancellationToken = default)
        => Task.FromResult(Guid.NewGuid());

    public Task ReceiveWebhookAsync(string partnerName, string payload, string? signature, string? timestamp, string? nonce, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<int> ProcessOutboxAsync(int take = 50, CancellationToken cancellationToken = default)
        => Task.FromResult(1);
}
