using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SubscriptionPlatform.Api.Data;
using SubscriptionPlatform.Api.Repositories;
using SubscriptionPlatform.Api.Services;

namespace SubscriptionPlatform.Api;

public sealed class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        ValidateConfiguration();
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddSingleton<ConnectionFactory>();
        services.AddControllers();
        services.AddOpenApi();
        services.AddHealthChecks().AddCheck<MySqlHealthCheck>("mysql");
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration["Authentication:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Authentication:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:SigningKey"]!)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
        services.AddAuthorization(options =>
        {
            options.AddPolicy("PlatformAdmin", policy => policy.RequireAuthenticatedUser().RequireClaim("role", "platform_admin"));
            options.AddPolicy("Application", policy => policy.RequireAuthenticatedUser().RequireClaim("role", "application"));
            options.AddPolicy("Customer", policy => policy.RequireAuthenticatedUser().RequireClaim("role", "customer"));
        });
        services.AddRateLimiter(options => options.AddFixedWindowLimiter("api", limiter =>
        {
            limiter.PermitLimit = 120;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueLimit = 0;
        }));

        services.AddScoped<RazorpayService>();
        services.AddScoped<IdentityRepository>();
        services.AddScoped<CatalogRepository>();
        services.AddScoped<SubscriptionRepository>();
        services.AddScoped<PaymentRepository>();
        services.AddScoped<IdentityService>();
        services.AddScoped<CatalogService>();
        services.AddScoped<SubscriptionService>();
        services.AddScoped<PaymentService>();
        services.AddHostedService<SubscriptionMaintenanceWorker>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseExceptionHandler();

        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers().RequireRateLimiting("api");
            endpoints.MapOpenApi();
        });
    }

    private void ValidateConfiguration()
    {
        var connection = configuration.GetConnectionString("SubscriptionDatabase")
            ?? throw new InvalidOperationException("ConnectionStrings:SubscriptionDatabase is required.");
        if (connection.Contains("change-me", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Configure a real MySQL password in ConnectionStrings:SubscriptionDatabase.");

        var signingKey = configuration["Authentication:SigningKey"]
            ?? throw new InvalidOperationException("Authentication:SigningKey is required.");
        if (signingKey.Contains("replace-with", StringComparison.OrdinalIgnoreCase) || Encoding.UTF8.GetByteCount(signingKey) < 32)
            throw new InvalidOperationException("Authentication:SigningKey must be a configured secret of at least 32 bytes.");

        var adminKey = configuration["Authentication:BootstrapAdminKey"]
            ?? throw new InvalidOperationException("Authentication:BootstrapAdminKey is required.");
        if (adminKey.Contains("replace-this", StringComparison.OrdinalIgnoreCase) || adminKey.Length < 32)
            throw new InvalidOperationException("Authentication:BootstrapAdminKey must be a configured secret of at least 32 characters.");
    }
}
