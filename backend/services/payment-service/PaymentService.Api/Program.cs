using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using PaymentService.Api.Data;
using PaymentService.Api.Endpoints;
using PaymentService.Api.Options;
using PaymentService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<PaymentDbContext>("shoppingcart");

builder.Services.Configure<PaymentGatewayOptions>(builder.Configuration.GetSection(PaymentGatewayOptions.SectionName));
builder.Services.AddScoped<PaymentManager>();

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
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapPaymentEndpoints();

app.Run();
