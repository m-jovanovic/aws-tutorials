using Microsoft.EntityFrameworkCore;

namespace VerticalSlices.Products;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; init; }
}
