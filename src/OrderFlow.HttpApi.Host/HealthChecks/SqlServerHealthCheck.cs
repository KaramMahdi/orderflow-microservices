using Domain.Shared.Layer.Constants;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrderFlow.HttpApi.Host.HealthChecks;

/// <summary>
/// Reports healthy when SQL Server accepts a connection and answers a trivial query.
/// A failure throws; the health check service turns the exception into Unhealthy and logs it.
/// </summary>
internal sealed class SqlServerHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public const string Name = "sqlserver";

    private const string ProbeQuery = "SELECT 1";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(configuration.GetConnectionString(ConnectionStringNames.Default));
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = ProbeQuery;
        await command.ExecuteScalarAsync(cancellationToken);

        return HealthCheckResult.Healthy();
    }
}
