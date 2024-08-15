using MassTransit;
using Stocks.Api.Orders;
using Stocks.Api.Stocks;
using Stocks.Platform.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configure =>
{
    //configure.SetKebabCaseEndpointNameFormatter();

    configure.AddConsumer<PurchaseOrderSentConsumer>().Endpoint(e => e.InstanceId = "stocks");

    configure.UsingAmazonSqs((context, cfg) =>
    {
        cfg.Host("eu-central-1", h =>
        {
            h.AccessKey(builder.Configuration["AmazonSqs:AccessKey"]!);
            h.SecretKey(builder.Configuration["AmazonSqs:SecretKey"]!);

            h.Scope("stocks-platform", true);
        });

        cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("stocks-platform-", false));
    });
});

builder.Services.AddHttpClient<StocksClient>(httpClient =>
{
    httpClient.BaseAddress = new Uri("https://www.alphavantage.co/query");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("stocks/{ticker}", async (string ticker, StocksClient stocksClient) =>
{
    var stockPriceResponse = await stocksClient.GetDataForTicker(ticker);

    return stockPriceResponse is not null ? Results.Ok(stockPriceResponse) : Results.NotFound();
});

app.MapPost("stocks", async (PurchaseOrderRequest request, IPublishEndpoint publishEndpoint) =>
{
    var order = new Order
    {
        Id = Guid.NewGuid(),
        Ticker = request.Ticker,
        LimitPrice = request.LimitPrice,
        Quantity = request.Quantity
    };

    OrdersDb.Instance.TryAdd(order.Id, order);

    await publishEndpoint.Publish(new PurchaseOrderSent(order.Id));

    return Results.Ok(order);
});

app.MapGet("orders/{id}", (Guid id) =>
{
    var order = OrdersDb.Instance.GetValueOrDefault(id);

    return order is not null ? Results.Ok(order) : Results.NotFound();
});

app.Run();
