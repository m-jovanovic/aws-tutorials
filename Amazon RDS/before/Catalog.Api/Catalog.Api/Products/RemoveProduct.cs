using VerticalSlices.Endpoints;

namespace VerticalSlices.Products;

public static class RemoveProduct
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("products/{id}", Handler).WithTags("Products");
        }
    }

    public static async Task<IResult> Handler(int id, AppDbContext context)
    {
        var product = await context.Products.FindAsync(id);

        if (product is null)
        {
            return Results.NotFound();
        }

        context.Remove(product);

        await context.SaveChangesAsync();

        return Results.NoContent();
    }
}
