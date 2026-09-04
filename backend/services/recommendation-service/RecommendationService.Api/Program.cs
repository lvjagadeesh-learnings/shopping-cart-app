using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using BuildingBlocks.Events;
using Microsoft.EntityFrameworkCore;
using RecommendationService.Api.Clients;
using RecommendationService.Api.Data;
using RecommendationService.Api.Endpoints;
using RecommendationService.Api.EventHandlers;
using RecommendationService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<RecommendationDbContext>("shoppingcart");

var catalogBaseUrl = builder.Configuration["Services:CatalogService:BaseUrl"] ?? "http://catalog-service";
builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
    client.BaseAddress = new Uri(catalogBaseUrl));

builder.Services.AddScoped<RecommendationManager>();

builder.Services.AddEventSubscriber<OrderPlacedEvent, OrderPlacedRecommendationHandler>(builder.Configuration, "Sqs:OrderPlacedQueueUrl");

builder.Services.AddSharedJwtAuthentication(builder.Configuration);
builder.Services.AddApiExceptionHandling();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RecommendationDbContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapRecommendationEndpoints();

app.Run();
