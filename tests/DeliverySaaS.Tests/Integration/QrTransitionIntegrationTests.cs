using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace DeliverySaaS.Tests.Integration;

public class QrTransitionIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public QrTransitionIntegrationTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task QrTransition_WithTransitionPermission_ReturnsOk()
    {
        var token = TestApiFactory.CreateJwt(
            new Claim(ClaimTypes.Role, "BranchUser"),
            new Claim("tenant_id", Guid.NewGuid().ToString()),
            new Claim("branch_id", Guid.NewGuid().ToString()),
            new Claim("permission", "OrdersView"),
            new Claim("permission", "OrdersTransition"));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var orderId = Guid.NewGuid();
        var payload = "{\"toState\":2}";
        var response = await _client.PostAsync($"/api/orders/{orderId}/transition", new StringContent(payload, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
