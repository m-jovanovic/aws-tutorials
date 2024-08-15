using MassTransit;
using RiskManagement.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configure =>
{
    //configure.SetKebabCaseEndpointNameFormatter();

    configure.AddConsumer<OrderFilledConsumer>().Endpoint(e => e.InstanceId = "risk-management");

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
