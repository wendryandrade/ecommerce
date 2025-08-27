using System.Net;
using System.Net.Http.Json;
using Ecommerce.API.Resources.Auth.DTOs.Requests;
using Ecommerce.API.Resources.Auth.DTOs.Responses;
using Ecommerce.API.Resources.Products.DTOs.Requests;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.API.IntegrationTests.Resources.Products.Controllers
{
    public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ProductsControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetById_ShouldReturnProduct_WhenProductExists()
        {
            var category = new Category { Name = "GetById Category", Description = "Desc" };
            var product = new Product { Name = "GetById Product", Description = "Desc", Price = 99.99m, StockQuantity = 5, Category = category };

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                // Seed admin e customer para login em outros testes
                var adminUser = new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), FirstName = "Admin", LastName = "User", Email = "admin@test.com", Role = "Admin" };
                var customerUser = new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), FirstName = "Customer", LastName = "User", Email = "customer@test.com", Role = "Customer" };
                adminUser.PasswordHash = GeneratePasswordHash("Password123!");
                customerUser.PasswordHash = GeneratePasswordHash("Password123!");
                dbContext.Users.AddRange(adminUser, customerUser);

                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync();
            }

            var response = await _client.GetAsync($"/api/products/{product.Id}");
            response.EnsureSuccessStatusCode();
            var productResponse = await response.Content.ReadFromJsonAsync<Ecommerce.API.Resources.Products.DTOs.Responses.ProductResponse>();
            Assert.NotNull(productResponse);
            Assert.Equal(product.Name, productResponse!.Name);
        }

        [Fact]
        public async Task Create_Product_ShouldReturnForbidden_WhenUserIsNotAdmin()
        {
            // login como customer
            var loginRequest = new LoginRequest { Email = "customer@test.com", Password = "Password123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();
            var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var createRequest = new CreateProductRequest
            {
                Name = "Forbidden Product",
                Description = "Description",
                Price = 10.0m,
                StockQuantity = 50,
                CategoryId = Guid.NewGuid()
            };

            var response = await _client.PostAsJsonAsync("/api/products", createRequest);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNotExists()
        {
            var res = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }

        [Fact]
        public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
        {
            var create = new CreateProductRequest { Name = "N", Description = "D", Price = 1, StockQuantity = 1, CategoryId = Guid.NewGuid() };
            var res = await _client.PostAsJsonAsync("/api/products", create);
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        public async Task Create_ShouldReturnCreated_WhenAdmin()
        {
            // Garante uma categoria válida no banco para satisfazer a FK
            Guid categoryId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var category = new Category { Id = Guid.NewGuid(), Name = "Cat Teste", Description = "Desc" };
                db.Categories.Add(category);
                await db.SaveChangesAsync();
                categoryId = category.Id;
            }

            var loginRequest = new LoginRequest { Email = "admin@test.com", Password = "Password123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();
            var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var create = new CreateProductRequest { Name = "Product X", Description = "D", Price = 9.99m, StockQuantity = 10, CategoryId = categoryId };
            var res = await _client.PostAsJsonAsync("/api/products", create);
            Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        }

        private static string GeneratePasswordHash(string password)
        {
            byte[] salt = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(salt);
            var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[52];
            Array.Copy(salt, 0, hashBytes, 0, 32);
            Array.Copy(hash, 0, hashBytes, 32, 20);
            return Convert.ToBase64String(hashBytes);
        }
    }
}