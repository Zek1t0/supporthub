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
        // 1) Arrange: creo un ticket
        var create = await _client.PostAsJsonAsync("/tickets", new { title = "seed from list test" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<TicketResponse>();
        Assert.NotNull(created);

        // 2) Act: pido la lista
        var res = await _client.GetAsync("/tickets");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var list = await res.Content.ReadFromJsonAsync<List<TicketResponse>>();
        Assert.NotNull(list);

        // 3) Assert: la lista contiene el ticket que creé
        Assert.Contains(list!, t => t.id == created!.id);
    }

    [Fact]
    public async Task GET_ticket_by_id_returns_200_when_ex()
    {
        // Se crea un ticket primero
        var create = await _client.PostAsJsonAsync("/tickets", new { title = "Buscar por id" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        // Se obtiene el id del ticket creado leyendo el body
        var createdTicket = await create.Content.ReadFromJsonAsync<TicketResponse>();
        Assert.NotNull(createdTicket);

        // Pedimos el ticket por id
        var get = await _client.GetAsync($"/tickets/{createdTicket!.id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }
    private record TicketResponse(Guid id, string title, string status, string priority);

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
    [Fact]
    public async Task PATCH_ticket_status_changes_status()
    {
        var create = await _client.PostAsJsonAsync("/tickets", new { title = "Cambiar estado" });
        var created = await create.Content.ReadFromJsonAsync<TicketResponse>();
        Assert.NotNull(created);

        var patch = await _client.PatchAsJsonAsync($"/tickets/{created!.id}/status", new { status = "Resolved" });
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);
    }

}

