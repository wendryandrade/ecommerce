using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Infrastructure;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Data.Common;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private DbConnection _connection;

    // Mocks públicos para os testes acessarem
    public readonly Mock<IShippingService> MockShippingService = new();
    public readonly Mock<IPaymentService> MockPaymentService = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "C0nF1d3nt1al_T3st_K3y_Th1s_1s_V3ry_S3cur3_And_L0ng_3n0ugh_F0r_HS256" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            });
        });

        // Isso garante que as modificações rodem DEPOIS da configuração normal da API.
        builder.ConfigureTestServices(services =>
        {
            // O bloco para remover os serviços reais e adicionar os Mocks
            // --- INÍCIO DA INJEÇÃO DOS MOCKS ---
            var shippingDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IShippingService));
            if (shippingDescriptor != null) services.Remove(shippingDescriptor);

            var paymentDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPaymentService));
            if (paymentDescriptor != null) services.Remove(paymentDescriptor);

            services.AddScoped(_ => MockShippingService.Object);
            services.AddScoped(_ => MockPaymentService.Object);
            // --- FIM DA INJEÇÃO DOS MOCKS ---

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // ADICIONE ISSO (4/4): Uma pequena correção para evitar recriar a conexão
            var connectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));
            if (connectionDescriptor != null)
                services.Remove(connectionDescriptor);

            services.AddSingleton<DbConnection>(container =>
            {
                if (_connection == null)
                {
                    _connection = new SqliteConnection("DataSource=:memory:");
                    _connection.Open();
                }
                return _connection;
            });

            services.AddDbContext<AppDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });

            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();

                // Método para gerar hash no formato caseiro (igual login)
                string GeneratePasswordHash(string password)
                {
                    byte[] salt = new byte[16];
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(salt);
                    }

                    using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
                    {
                        byte[] hash = pbkdf2.GetBytes(20);

                        byte[] hashBytes = new byte[36];
                        Array.Copy(salt, 0, hashBytes, 0, 16);
                        Array.Copy(hash, 0, hashBytes, 16, 20);

                        return Convert.ToBase64String(hashBytes);
                    }
                }

                // Usuário Admin
                var adminUser = new User
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@test.com",
                    Role = "Admin",
                    Addresses = new List<Address>()
                };
                adminUser.PasswordHash = GeneratePasswordHash("Password123!");

                // Usuário Customer
                var customerUser = new User
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    FirstName = "Customer",
                    LastName = "User",
                    Email = "customer@test.com",
                    Role = "Customer",
                    Addresses = new List<Address>()
                };
                customerUser.PasswordHash = GeneratePasswordHash("Password123!");

                db.Users.AddRange(adminUser, customerUser);
                db.SaveChanges();
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}