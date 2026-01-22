using SupportHub.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var tickets = new List<Ticket>
{
    new Ticket { Id = Guid.NewGuid(), Title = "No puedo iniciar sesión", Status = "Open", Priority = "High" },
    new Ticket { Id = Guid.NewGuid(), Title = "Bug en el checkout", Status = "InProgress", Priority = "Urgent" }
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// POST /tickets: create a new support ticket
app.MapPost("/tickets", (CreateTicketRequest request) =>
{
    // Basic validation
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest(new { error = "Title is required." });
    }

    var ticket = new Ticket
    {
        Id = Guid.NewGuid(),
        Title = request.Title.Trim(),
        Status = "Open",
        Priority = "Medium",
        CreatedAt = DateTime.UtcNow
    };

    tickets.Add(ticket);
// 201 Created + get ticket by id endpoint
    return Results.Created($"/tickets/{ticket.Id}", ticket);
});

app.MapGet("/tickets", () => Results.Ok(tickets));

app.MapGet("/tickets/{id:guid}", (Guid id) =>
{
    var ticket = tickets.FirstOrDefault(t => t.Id == id);
    return ticket is null ? Results.NotFound() : Results.Ok(ticket);
});

app.MapPatch("/tickets/{id:guid}/status", (Guid id, UpdateTicketStatusRequest request) =>
{
    var ticket = tickets.FirstOrDefault(t => t.Id == id);
    if (ticket is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(request.Status))
        return Results.BadRequest(new { error = "Status is required." });

    // estados válidos
    var valid = new[] { "Open", "InProgress", "Resolved" };
    if (!valid.Contains(request.Status.Trim()))
        return Results.BadRequest(new { error = "Invalid status." });

    ticket.Status = request.Status.Trim();
    return Results.Ok(ticket);
});

app.Run();


record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program { }

