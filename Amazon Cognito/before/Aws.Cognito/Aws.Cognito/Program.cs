using Aws.Cognito;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WeatherService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", (WeatherService weatherService) => weatherService.GetForecast())
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

