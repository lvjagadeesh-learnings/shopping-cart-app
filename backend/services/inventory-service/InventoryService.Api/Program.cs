using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using InventoryService.Api.Clients;
using InventoryService.Api.Data;
using InventoryService.Api.Endpoints;
using InventoryService.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<InventoryDbContext>("shoppingcart");

// "catalog-service" resolves via Aspire service discovery locally; configure
// "Services:CatalogService:BaseUrl" to override in non-Aspire environments (e.g. AWS ECS/ALB).
var catalogBaseUrl = builder.Configuration["Services:CatalogService:BaseUrl"] ?? "http://catalog-service";
builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
    client.BaseAddress = new Uri(catalogBaseUrl));

builder.Services.AddScoped<InventoryManager>();

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
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await db.Database.MigrateAsync();

    var catalogClient = scope.ServiceProvider.GetRequiredService<ICatalogServiceClient>();
    await InventoryDataSeeder.SeedAsync(db, catalogClient, CancellationToken.None);
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapInventoryEndpoints();

app.Run();
