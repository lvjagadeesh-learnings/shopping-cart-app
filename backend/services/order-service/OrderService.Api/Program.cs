using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using BuildingBlocks.Events;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Clients;
using OrderService.Api.Data;
using OrderService.Api.Endpoints;
using OrderService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<OrderDbContext>("shoppingcart");

// Downstream service URLs resolve via Aspire service discovery locally ("http://<resource-name>"
// matches the AppHost resource name); configure "Services:<Service>:BaseUrl" to override in
// non-Aspire environments (e.g. AWS ECS/ALB).
var cartBaseUrl = builder.Configuration["Services:CartService:BaseUrl"] ?? "http://cart-service";
builder.Services.AddHttpClient<ICartServiceClient, CartServiceClient>(client =>
    client.BaseAddress = new Uri(cartBaseUrl));

var inventoryBaseUrl = builder.Configuration["Services:InventoryService:BaseUrl"] ?? "http://inventory-service";
builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
    client.BaseAddress = new Uri(inventoryBaseUrl));

var paymentBaseUrl = builder.Configuration["Services:PaymentService:BaseUrl"] ?? "http://payment-service";
builder.Services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>(client =>
    client.BaseAddress = new Uri(paymentBaseUrl));

builder.Services.AddEventPublishing(builder.Configuration);

builder.Services.AddScoped<OrderManager>();

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
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
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
app.MapOrderEndpoints();

app.Run();
