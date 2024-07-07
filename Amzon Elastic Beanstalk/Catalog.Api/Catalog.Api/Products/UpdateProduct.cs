using Microsoft.AspNetCore.Http.HttpResults;
using VerticalSlices.Endpoints;

namespace VerticalSlices.Products;

public static class UpdateProduct
{
    public record Request(string Name, decimal Price);
    public record Response(int Id, string Name, decimal Price);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("products/{id}", Handler).WithTags("Products");
        }
    }

    public static async Task<Results<Ok<Response>, NotFound>> Handler(int id, Request request, AppDbContext context)
    {
        var product = await context.Products.FindAsync(id);

        if (product is null)
        {
            return TypedResults.NotFound();
        }

        product.Name = request.Name;
        product.Price = request.Price;

        await context.SaveChangesAsync();

        return TypedResults.Ok(new Response(product.Id, product.Name, product.Price));
    }
}
