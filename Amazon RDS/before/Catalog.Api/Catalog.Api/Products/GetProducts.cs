using Microsoft.EntityFrameworkCore;
using VerticalSlices.Endpoints;

namespace VerticalSlices.Products;

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

    public static async Task<IResult> Handler(AppDbContext context)
    {
        var products = await context.Products.ToListAsync();

        var responses = products.Select(p => new Response(p.Id, p.Name, p.Price)).ToList();

        return TypedResults.Ok(responses);
    }
}
