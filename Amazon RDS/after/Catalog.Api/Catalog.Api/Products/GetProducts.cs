using Catalog.Api.Database;
using Catalog.Api.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Products;

public static class GetProducts
{
    public record Response(int Id, string Name, decimal Price);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("products", Handler).WithTags("Products");
        }
    }

    public static async Task<IResult> Handler(ReadonlyAppDbContext context)
    {
        var products = await context.Products.ToListAsync();

        var responses = products.Select(p => new Response(p.Id, p.Name, p.Price)).ToList();

        return TypedResults.Ok(responses);
    }
}
