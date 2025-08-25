using Ecommerce.Infrastructure.Persistence.Context;
using Ecommerce.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.API.Extensions
{
    public static class SeedExtensions
    {
        public static async Task SeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();
            var seeder = new DatabaseSeeder(dbContext, logger);

            try
            {
                logger.LogInformation("=== INICIANDO SEED DO BANCO DE DADOS ===");
                
                // Aguardar um pouco para garantir que o banco está pronto
                await Task.Delay(2000);
                
                await seeder.SeedAsync();
                
                logger.LogInformation("=== SEED DO BANCO DE DADOS CONCLUÍDO ===");
            }
            catch (Exception ex)
            {
                // Não falhar a aplicação se o seed falhar
                logger.LogError(ex, "Erro durante o seed do banco de dados: {Message}", ex.Message);
                logger.LogError(ex, "StackTrace: {StackTrace}", ex.StackTrace);
            }
        }
    }
}


