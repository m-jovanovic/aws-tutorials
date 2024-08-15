using Catalog.Api.Products;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Database;

public class ReadonlyAppDbContext : DbContext
{
    public ReadonlyAppDbContext(DbContextOptions<ReadonlyAppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; init; }
}
