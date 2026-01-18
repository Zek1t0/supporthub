using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace SupportHub.Api.Tests;

public class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        var testingFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });

        _client = testingFactory.CreateClient();
    }


    [Fact]
    public async Task GET_weatherforecast_returns_200_ok()
    {
        var response = await _client.GetAsync("/weatherforecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GET_health_returns_200_ok()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    [Fact]
    public async Task GET_tickets_returns_200_and_list()
    {
        var response = await _client.GetAsync("/tickets");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(body);
        Assert.True(body!.Count > 0);
    }   
    [Fact]
    public async Task POST_tickets_with_title_returns_201_created()
    {
        var payload = new { title = "Ticket desde test"};

        var response = await _client.PostAsJsonAsync("/tickets", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    }
    [Fact]
    public async Task POST_tickets_without_title_returns_400()
    {
        var payload = new { title = ""};

        var response = await _client.PostAsJsonAsync("/tickets", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    }
}

