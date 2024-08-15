using Catalog.Api.Products;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; init; }
}
