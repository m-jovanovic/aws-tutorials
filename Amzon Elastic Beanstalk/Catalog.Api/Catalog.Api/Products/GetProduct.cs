using Microsoft.AspNetCore.Http.HttpResults;
using VerticalSlices.Endpoints;

namespace VerticalSlices.Products;

public static class GetProduct
{
    public record Response(int Id, string Name, decimal Price);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("products/{id}", Handler).WithTags("Products");
        }
    }

    public static async Task<Results<Ok<Response>, NotFound>> Handler(int id, AppDbContext context)
    {
        var product = await context.Products.FindAsync(id);

        if (product is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new Response(product.Id, product.Name, product.Price));
    }
}
