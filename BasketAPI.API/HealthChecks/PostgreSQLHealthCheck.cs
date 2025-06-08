using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace BasketAPI.API.HealthChecks;

public class PostgreSQLHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public PostgreSQLHealthCheck(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("PostgreSQL database is accessible");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL database is not accessible", ex);
        }
    }
}
