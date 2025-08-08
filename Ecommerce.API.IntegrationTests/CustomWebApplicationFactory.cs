using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using System.Security.Cryptography;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private DbConnection _connection;

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

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddSingleton<DbConnection>(_connection);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
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