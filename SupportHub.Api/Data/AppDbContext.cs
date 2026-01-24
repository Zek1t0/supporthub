using Microsoft.EntityFrameworkCore;
using SupportHub.Api.Models;

namespace SupportHub.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();
}
