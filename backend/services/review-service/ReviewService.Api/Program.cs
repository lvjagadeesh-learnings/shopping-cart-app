using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using ReviewService.Api.Clients;
using ReviewService.Api.Data;
using ReviewService.Api.Endpoints;
using ReviewService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<ReviewDbContext>("shoppingcart");

var catalogBaseUrl = builder.Configuration["Services:CatalogService:BaseUrl"] ?? "http://catalog-service";
builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
    client.BaseAddress = new Uri(catalogBaseUrl));

builder.Services.AddScoped<ReviewManager>();

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
    var db = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
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
app.MapReviewEndpoints();

app.Run();
