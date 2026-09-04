using AuthService.Api.Data;
using AuthService.Api.Domain;
using AuthService.Api.Endpoints;
using AuthService.Api.Services;
using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<AuthDbContext>("shoppingcart");

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<JwtTokenGenerator>();
builder.Services.AddSingleton<PasswordHasher<User>>();
builder.Services.AddScoped<AuthTokenService>();

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
    await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapAuthEndpoints();

app.Run();

