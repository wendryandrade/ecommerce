using Ecommerce.API.Resources.Auth.DTOs.Requests;
using Ecommerce.API.Resources.Auth.DTOs.Responses;
using Ecommerce.API.Resources.Products.DTOs.Requests;
using Ecommerce.API.Resources.Products.DTOs.Responses;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace Ecommerce.API.IntegrationTests.Resources.Products.Controllers
{
    public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        // O construtor prepara o banco de dados para os testes desta classe
        public ProductsControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                // Popula o banco com os usuários necessários para os testes
                var adminUser = new User
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@test.com",
                    Role = "Admin"
                };
                adminUser.PasswordHash = GeneratePasswordHash("Password123!");

                var customerUser = new User
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    FirstName = "Customer",
                    LastName = "User",
                    Email = "customer@test.com",
                    Role = "Customer"
                };
                customerUser.PasswordHash = GeneratePasswordHash("Password123!");

                dbContext.Users.AddRange(adminUser, customerUser);
                dbContext.SaveChanges();
            }
        }

        [Fact]
        public async Task GetById_ShouldReturnProduct_WhenProductExists()
        {
            // --- ARRANGE ---
            // 1. Crie os dados necessários direto no banco.
            var category = new Category { Name = "GetById Category", Description = "Desc" };
            var product = new Product { Name = "GetById Product", Description = "Desc", Price = 99.99m, StockQuantity = 5, Category = category };

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync();
            }

            // --- ACT ---
            var response = await _client.GetAsync($"/api/products/{product.Id}");

            // --- ASSERT ---
            response.EnsureSuccessStatusCode();
            var productResponse = await response.Content.ReadFromJsonAsync<ProductResponse>();
            Assert.NotNull(productResponse);
            Assert.Equal(product.Name, productResponse.Name);
        }

        [Fact]
        public async Task Create_Product_ShouldReturnForbidden_WhenUserIsNotAdmin()
        {
            // --- ARRANGE ---
            // 1. Autentique como um usuário "Customer" (não-admin).
            var loginRequest = new LoginRequest { Email = "customer@test.com", Password = "Password123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();
            var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 2. Prepare o request para criar o produto.
            var createRequest = new CreateProductRequest
            {
                Name = "Forbidden Product",
                Description = "Description",
                Price = 10.0m,
                StockQuantity = 50,
                CategoryId = Guid.NewGuid() // ID não precisa existir, pois a requisição falhará antes.
            };

            // --- ACT ---
            var response = await _client.PostAsJsonAsync("/api/products", createRequest);

            // --- ASSERT ---
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Método auxiliar para gerar um hash de senha válido.
        private static string GeneratePasswordHash(string password)
        {
            byte[] salt = new byte[32];
            RandomNumberGenerator.Fill(salt);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[52];
            Array.Copy(salt, 0, hashBytes, 0, 32);
            Array.Copy(hash, 0, hashBytes, 32, 20);
            return Convert.ToBase64String(hashBytes);
        }
    }
}