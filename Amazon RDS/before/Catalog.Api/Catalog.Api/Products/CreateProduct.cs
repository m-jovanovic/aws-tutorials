using FluentValidation;
using VerticalSlices.Endpoints;

namespace VerticalSlices.Products;

public static class CreateProduct
{
    public record Request(string Name, decimal Price);
    public record Response(int Id, string Name, decimal Price);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Name).NotEmpty();
            RuleFor(r => r.Price).GreaterThan(0);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("products", Handler).WithTags("Products");
        }
    }

    public static async Task<IResult> Handler(Request request, AppDbContext context, IValidator<Request> validator)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var product = new Product { Name = request.Name, Price = request.Price };

        context.Products.Add(product);

        await context.SaveChangesAsync();

        return Results.Ok(new Response(product.Id, product.Name, product.Price));
    }
}
