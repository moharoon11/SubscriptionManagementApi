using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SubscriptionPlatform.Api.Data;

namespace SubscriptionPlatform.Api.Services;

public sealed class MySqlHealthCheck(ConnectionFactory connections):IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,CancellationToken cancellationToken=default){try{await using var db=connections.Create();await db.OpenAsync(cancellationToken);await db.ExecuteScalarAsync<int>(new CommandDefinition("SELECT 1",cancellationToken:cancellationToken));return HealthCheckResult.Healthy();}catch(Exception ex){return HealthCheckResult.Unhealthy("MySQL unavailable",ex);}}
}
