using Ecommerce.API.Resources.Auth.DTOs.Requests;
using Ecommerce.API.Resources.Auth.DTOs.Responses;
using Ecommerce.API.Resources.Carts.DTOs.Requests;
using Ecommerce.API.Resources.Orders.DTOs.Requests;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Ecommerce.API.IntegrationTests.Resources.Orders.Controllers
{
    public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public OrdersControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnCreated_WhenCheckoutIsValidAndUserIsAuthenticated()
        {
            // --- ARRANGE ---

            var expectedShippingCost = 15.50m;
            var expectedTransactionId = "pi_mock_transaction_12345";

            _factory.MockShippingService
                .Setup(s => s.CalculateShippingCostAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedShippingCost);

            _factory.MockPaymentService
                .Setup(p => p.ProcessPaymentAsync(It.IsAny<decimal>(), "brl", It.IsAny<string>()))
                .ReturnsAsync(expectedTransactionId);

            // Obtem o DbContext e limpa apenas os dados transacionais de testes anteriores
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Limpam apenas as tabelas relevantes para o teste.
            // Os usuários criados pela Factory permanecem intactos.
            dbContext.Orders.RemoveRange(dbContext.Orders);
            dbContext.Carts.RemoveRange(dbContext.Carts);
            dbContext.Products.RemoveRange(dbContext.Products);
            dbContext.Categories.RemoveRange(dbContext.Categories);
            await dbContext.SaveChangesAsync();

            // O usuário já foi criado pela Factory, então apenas é buscado
            var user = await dbContext.Users.FirstAsync(u => u.Email == "customer@test.com");
            var category = new Category { Name = "Test Category", Description = "A test description" };
            var product = new Product { Name = "Test Product", Description = "A test product description", Price = 100.00m, StockQuantity = 10, Category = category };
            var cart = new Cart { UserId = user.Id };
            cart.AddItem(product, 2);

            dbContext.Products.Add(product);
            dbContext.Carts.Add(cart);
            await dbContext.SaveChangesAsync();

            // Autenticar o usuário para obter um token JWT
            var loginRequest = new LoginRequest { Email = "customer@test.com", Password = "Password123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();
            var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Montar a requisição da API
            var createOrderRequest = new CreateOrderRequest
            {
                UserId = user.Id,
                ShippingAddress = new CreateOrderAddressRequest
                {
                    Street = "Rua do Teste",
                    City = "Cidade Teste",
                    State = "TS",
                    PostalCode = "98765432"
                },
                PaymentDetails = new CreateOrderPaymentRequest { PaymentMethod = PaymentMethod.CreditCard }
            };

            // --- ACT ---
            var response = await _client.PostAsJsonAsync("/api/orders/checkout", createOrderRequest);

            // --- ASSERT ---

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var expectedTotalAmount = (2 * 100.00m) + expectedShippingCost;

            _factory.MockShippingService.Verify(s => s.CalculateShippingCostAsync(It.IsAny<string>(), "98765432"), Times.Once);
            _factory.MockPaymentService.Verify(p => p.ProcessPaymentAsync(expectedTotalAmount, "brl", "pm_card_visa"), Times.Once);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var orderInDb = await assertDbContext.Orders
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == user.Id);

            Assert.NotNull(orderInDb);
            Assert.Equal(expectedTotalAmount, orderInDb.TotalAmount);
            Assert.Equal(OrderStatus.Paid, orderInDb.Status);
            Assert.NotNull(orderInDb.Payment);
            Assert.Equal(expectedTransactionId, orderInDb.Payment.TransactionId);

            var productInDb = await assertDbContext.Products.FindAsync(product.Id);
            Assert.NotNull(productInDb);
            Assert.Equal(8, productInDb.StockQuantity);
        }
    }
}