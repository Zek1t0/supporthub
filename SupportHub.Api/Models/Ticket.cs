namespace SupportHub.Api.Models;

public class Ticket
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}






