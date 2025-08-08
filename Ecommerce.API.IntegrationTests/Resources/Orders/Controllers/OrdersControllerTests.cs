using Ecommerce.API.Resources.Auth.DTOs.Requests;
using Ecommerce.API.Resources.Auth.DTOs.Responses;
using Ecommerce.API.Resources.Carts.DTOs.Requests;
using Ecommerce.API.Resources.Orders.DTOs.Requests;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace Ecommerce.API.IntegrationTests.Resources.Orders.Controllers
{
    public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        // O construtor prepara o banco de dados para os testes desta classe
        public OrdersControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            }
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnCreated_WhenCartIsNotEmpty()
        {
            // --- ARRANGE ---

            // 1. Crie os dados necessários (produto e usuário) direto no banco.
            var category = new Category { Name = "Order Test Category", Description = "Desc" };
            var product = new Product { Name = "Order Test Product", Description = "Desc", Price = 50.0m, StockQuantity = 20, Category = category };

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // Adiciona também os usuários aqui para garantir que o teste seja independente
                var customerUser = new User
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    FirstName = "Customer",
                    LastName = "OrderUser",
                    Email = "customer@test.com",
                    Role = "Customer"
                };
                // CORREÇÃO: Gera um hash de senha válido para o usuário de teste.
                customerUser.PasswordHash = GeneratePasswordHash("Password123!");

                dbContext.Users.Add(customerUser);
                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync();
            }

            // 2. Autentique o usuário
            var loginRequest = new LoginRequest { Email = "customer@test.com", Password = "Password123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();
            var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 3. Adicione um item ao carrinho (esta parte precisa usar a API, pois é parte do fluxo)
            var addToCartRequest = new AddCartItemRequest { ProductId = product.Id, Quantity = 1 };
            var addToCartResponse = await _client.PostAsJsonAsync("/api/cart/items", addToCartRequest);
            addToCartResponse.EnsureSuccessStatusCode();

            // 4. Prepare o request para criar o pedido
            var createOrderRequest = new CreateOrderRequest
            {
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                ShippingAddress = new CreateOrderAddressRequest
                {
                    Street = "Rua do Pedido",
                    City = "Cidade Teste",
                    State = "TS",
                    PostalCode = "98765-432"
                },
                PaymentDetails = new CreateOrderPaymentRequest
                {
                    PaymentMethod = PaymentMethod.CreditCard
                }
            };

            // --- ACT ---
            var response = await _client.PostAsJsonAsync("/api/orders/checkout", createOrderRequest);

            // --- ASSERT ---
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // Método auxiliar para gerar um hash de senha válido.
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
