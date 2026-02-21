using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace DeliverySaaS.Tests.Integration;

public class JwtAuthIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public JwtAuthIntegrationTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MobileEndpoint_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/mobile/pickup/tasks");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MobileEndpoint_WithJwt_ReturnsOk()
    {
        var token = TestApiFactory.CreateJwt(
            new Claim(ClaimTypes.Role, "BranchUser"),
            new Claim("tenant_id", Guid.NewGuid().ToString()),
            new Claim("branch_id", Guid.NewGuid().ToString()));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/mobile/pickup/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
