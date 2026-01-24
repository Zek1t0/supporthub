using SupportHub.Api.Models;
using Microsoft.EntityFrameworkCore;
using SupportHub.Api.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

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
app.MapPost("/tickets", async (AppDbContext db, CreateTicketRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
        return Results.BadRequest(new { error = "Title is required" });

    var ticket = new Ticket
    {
        Id = Guid.NewGuid(),
        Title = request.Title.Trim(),
        Status = "Open",
        Priority = "Medium",
        CreatedAt = DateTime.UtcNow
    };

    db.Tickets.Add(ticket);
    await db.SaveChangesAsync();

    return Results.Created($"/tickets/{ticket.Id}", ticket);
});

app.MapGet("/tickets", async (AppDbContext db) =>
{
    var list = await db.Tickets.OrderByDescending(t => t.CreatedAt).ToListAsync();
    return Results.Ok(list);
});

app.MapGet("/tickets/{id:guid}", async (AppDbContext db, Guid id) =>
{
    var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
    return ticket is null ? Results.NotFound() : Results.Ok(ticket);
});


app.MapPatch("/tickets/{id:guid}/status", async (AppDbContext db, Guid id, UpdateTicketStatusRequest request) =>
{
    var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
    if (ticket is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(request.Status))
        return Results.BadRequest(new { error = "Status is required." });

    var status = request.Status.Trim();
    var valid = new[] { "Open", "InProgress", "Resolved" };
    if (!valid.Contains(status))
        return Results.BadRequest(new { error = "Invalid status." });

    ticket.Status = status;
    await db.SaveChangesAsync();

    return Results.Ok(ticket);
});


app.Run();


record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program { }

