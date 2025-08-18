using Ecommerce.Infrastructure.Persistence.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Ecommerce.API.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Thread.Sleep(15000); // espera inicial do SQL Server no Docker

            var retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetry(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning(ex, "Erro ao conectar ao banco de dados. Tentando novamente em {time}", time);
                    });

            retryPolicy.Execute(() =>
            {
                dbContext.Database.Migrate();
            });
        }
    }
}
