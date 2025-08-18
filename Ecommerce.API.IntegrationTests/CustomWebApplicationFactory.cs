using Ecommerce.API.Consumers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Infrastructure;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence.Context;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private DbConnection _connection;

    // Mocks públicos
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

        builder.ConfigureTestServices(services =>
        {
            // Remover implementações reais
            var shippingDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IShippingService));
            if (shippingDescriptor != null) services.Remove(shippingDescriptor);

            var paymentDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPaymentService));
            if (paymentDescriptor != null) services.Remove(paymentDescriptor);

            // Injetar mocks
            services.AddScoped(_ => MockShippingService.Object);
            services.AddScoped(_ => MockPaymentService.Object);

            // Remover DbContext real
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));
            if (dbConnectionDescriptor != null) services.Remove(dbConnectionDescriptor);

            // Adicionar SQLite in-memory
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

            // MassTransit TestHarness
            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<OrderConsumer>();
            });

            // Popula o banco com dados iniciais
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();

                if (!db.Users.Any())
                {
                    string GeneratePasswordHash(string password)
                    {
                        byte[] salt = new byte[16];
                        using (var rng = RandomNumberGenerator.Create()) { rng.GetBytes(salt); }
                        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
                        {
                            byte[] hash = pbkdf2.GetBytes(20);
                            byte[] hashBytes = new byte[36];
                            Array.Copy(salt, 0, hashBytes, 0, 16);
                            Array.Copy(hash, 0, hashBytes, 16, 20);
                            return Convert.ToBase64String(hashBytes);
                        }
                    }

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
            }
        });
    }

    // Dispose da conexão SQLite
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}
