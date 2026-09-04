using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using CartService.Api.Clients;
using CartService.Api.Data;
using CartService.Api.Endpoints;
using CartService.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<CartDbContext>("shoppingcart");

// "catalog-service" resolves via Aspire service discovery locally; configure
// "Services:CatalogService:BaseUrl" to override in non-Aspire environments (e.g. AWS ECS/ALB).
var catalogBaseUrl = builder.Configuration["Services:CatalogService:BaseUrl"] ?? "http://catalog-service";
builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
    client.BaseAddress = new Uri(catalogBaseUrl));

builder.Services.AddScoped<CartManager>();

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
    var db = scope.ServiceProvider.GetRequiredService<CartDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapCartEndpoints();

app.Run();

