using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using BuildingBlocks.Events;
using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Data;
using NotificationService.Api.Endpoints;
using NotificationService.Api.EventHandlers;
using NotificationService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<NotificationDbContext>("shoppingcart");

builder.Services.AddScoped<NotificationManager>();

builder.Services.AddEventSubscriber<OrderPlacedEvent, OrderPlacedNotificationHandler>(builder.Configuration, "Sqs:OrderPlacedQueueUrl");
builder.Services.AddEventSubscriber<OrderStatusChangedEvent, OrderStatusChangedNotificationHandler>(builder.Configuration, "Sqs:OrderStatusChangedQueueUrl");

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
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapNotificationEndpoints();

app.Run();
