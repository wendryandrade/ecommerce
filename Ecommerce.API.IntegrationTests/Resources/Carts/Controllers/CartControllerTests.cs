using Ecommerce.API.Resources.Auth.DTOs.Requests;
using Ecommerce.API.Resources.Auth.DTOs.Responses;
using Ecommerce.API.Resources.Carts.DTOs.Requests;
using Ecommerce.API.Resources.Carts.DTOs.Responses;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace Ecommerce.API.IntegrationTests.Resources.Carts.Controllers
{
    public class CartControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        // O Construtor agora é responsável por preparar o banco de dados
        public CartControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            // Usamos o provedor de serviços da factory para obter o DbContext
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // 1. Garante um banco de dados limpo para cada execução da classe de teste
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                // 2. Popula o banco com os usuários necessários para os testes
                var adminUser = new User
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@test.com",
                    Role = "Admin"
                };
                // CORREÇÃO: Chama o método de hash local desta classe
                adminUser.PasswordHash = GeneratePasswordHash("Password123!");

                var customerUser = new User
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    FirstName = "Customer",
                    LastName = "User",
                    Email = "customer@test.com",
                    Role = "Customer"
                };
                // CORREÇÃO: Chama o método de hash local desta classe
                customerUser.PasswordHash = GeneratePasswordHash("Password123!");

                dbContext.Users.AddRange(adminUser, customerUser);
                dbContext.SaveChanges();
            }
        }

        [Fact]
        public async Task AddItem_ShouldReturnOk_WhenUserIsAuthenticated()
        {
            // --- ARRANGE ---

            // 1. Crie os dados necessários (produto) direto no banco.
            //    É mais rápido e confiável do que usar a API.
            var category = new Category { Name = "Test Category", Description = "A description for the test category." };
            // CORREÇÃO: Adicionada a propriedade 'Description' ao produto.
            var product = new Product { Name = "Test Product", Description = "A description for the test product.", Price = 10.0m, StockQuantity = 10, Category = category };

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync();
            }

            // 2. Autentique o usuário
            var customerLoginRequest = new LoginRequest { Email = "customer@test.com", Password = "Password123!" };
            var customerLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", customerLoginRequest);
            customerLoginResponse.EnsureSuccessStatusCode(); // Garante que o login funcionou
            var customerToken = (await customerLoginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

            // 3. Prepare o request para o carrinho
            var addToCartRequest = new AddCartItemRequest
            {
                ProductId = product.Id,
                Quantity = 2
            };

            // --- ACT ---
            var response = await _client.PostAsJsonAsync("/api/cart/items", addToCartRequest);

            // --- ASSERT ---
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetMyCart_ShouldReturnEmptyCart_WhenUserHasNoItems()
        {
            // Este teste agora funciona corretamente porque o construtor garante
            // um banco de dados limpo e populado apenas com o usuário.
            var loginRequest = new LoginRequest { Email = "customer@test.com", Password = "Password123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();

            var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var userId = Guid.Parse("00000000-0000-0000-0000-000000000002");

            var response = await _client.GetAsync("/api/cart");
            response.EnsureSuccessStatusCode();

            var cartResponse = await response.Content.ReadFromJsonAsync<CartResponse>();

            Assert.NotNull(cartResponse);
            Assert.Equal(userId, cartResponse!.UserId);
            Assert.Empty(cartResponse.Items);
        }

        // CORREÇÃO: Adicionado o método de hash como um método auxiliar privado
        private static string GeneratePasswordHash(string password)
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            return Convert.ToBase64String(hashBytes);
        }
    }
}