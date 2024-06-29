using Aws.Cognito;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WeatherService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.ConfigureOptions<JwtBearerConfigureOptions>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/weatherforecast", (WeatherService weatherService) => weatherService.GetForecast())
    .WithName("GetWeatherForecast")
    .WithOpenApi()
    .RequireAuthorization();

app.MapGet("/claims", (ClaimsPrincipal claims) => claims.Claims.Select(c => new { c.Type, c.Value }).ToArray())
    .RequireAuthorization()
    .WithName("GetClaims")
    .WithOpenApi();

app.UseAuthentication();

app.UseAuthorization();

app.Run();
