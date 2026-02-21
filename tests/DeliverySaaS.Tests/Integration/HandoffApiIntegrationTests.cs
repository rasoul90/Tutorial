using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace DeliverySaaS.Tests.Integration;

public class HandoffApiIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public HandoffApiIntegrationTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task OutboundHandoff_WithJwt_ReturnsOk()
    {
        var token = TestApiFactory.CreateJwt(
            new Claim(ClaimTypes.Role, "CompanyAdmin"),
            new Claim("tenant_id", Guid.NewGuid().ToString()),
            new Claim("branch_id", Guid.NewGuid().ToString()));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var orderId = Guid.NewGuid();
        var payload = $"{{\"governorateId\":\"{Guid.NewGuid()}\"}}";
        var response = await _client.PostAsync($"/api/integration/orders/{orderId}/outbound-handoff", new StringContent(payload, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
