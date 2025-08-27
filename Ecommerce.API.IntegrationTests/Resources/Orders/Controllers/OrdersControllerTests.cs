using Ecommerce.API.Consumers;
using Ecommerce.API.Resources.Auth.DTOs.Requests;
using Ecommerce.API.Resources.Auth.DTOs.Responses;
using Ecommerce.API.Resources.Orders.DTOs.Requests;
using Ecommerce.Application.Features.Orders.Events;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Persistence.Context;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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

            // Configura os mocks do consumer
            _factory.MockShippingService
                .Setup(s => s.CalculateShippingCostAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(0m);

            _factory.MockShippingService
                .Setup(s => s.CalculateShippingWithDetailsFromStoreAsync(It.IsAny<string>()))
                .ReturnsAsync((0m, 3, false, new Application.Interfaces.AddressInfo(), new Application.Interfaces.AddressInfo()));

            _factory.MockShippingService
                .Setup(s => s.GetStoreZipCode())
                .Returns("01310-100");

            _factory.MockPaymentService
                .Setup(p => p.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("mock_tx_id");
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnCreated_AndConsumerProcessesMessage()
        {
            var harness = _factory.Services.GetRequiredService<ITestHarness>();
            await harness.Start();

            try
            {
                // --- Prepara dados ---
                using var scope = _factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.Orders.RemoveRange(dbContext.Orders);
                dbContext.Carts.RemoveRange(dbContext.Carts);
                dbContext.Products.RemoveRange(dbContext.Products);
                dbContext.Categories.RemoveRange(dbContext.Categories);
                await dbContext.SaveChangesAsync();

                var user = await dbContext.Users.FirstAsync(u => u.Email == "customer@test.com");

                var category = new Category { Name = "Test", Description = "Desc" };
                var product = new Product
                {
                    Name = "Test",
                    Description = "Desc",
                    Price = 100.00m,
                    StockQuantity = 10,
                    Category = category
                };

                var cart = new Cart { UserId = user.Id };
                cart.AddItem(product, 2);

                dbContext.Products.Add(product);
                dbContext.Carts.Add(cart);
                await dbContext.SaveChangesAsync();

                // --- Autentica ---
                var loginRequest = new LoginRequest { Email = "customer@test.com", Password = "Password123!" };
                var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
                loginResponse.EnsureSuccessStatusCode();
                var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // --- Cria pedido ---
                var createOrderRequest = new CreateOrderRequest
                {
                    ShippingAddress = new CreateOrderAddressRequest
                    {
                        Street = "Rua do Teste",
                        City = "Cidade Teste",
                        State = "TS",
                        PostalCode = "98765432"
                    }
                };

                var response = await _client.PostAsJsonAsync("/api/orders/checkout", createOrderRequest);
                response.EnsureSuccessStatusCode();

                // --- Aguarda consumo do evento ---
                var consumed = false;
                var eventTimeout = TimeSpan.FromSeconds(10);
                var eventStart = DateTime.UtcNow;
                
                while ((DateTime.UtcNow - eventStart) < eventTimeout && !consumed)
                {
                    consumed = await harness.Consumed.Any<OrderSubmissionEvent>();
                    if (!consumed)
                    {
                        await Task.Delay(100);
                    }
                }
                
                Assert.True(consumed, "O evento OrderSubmissionEvent não foi consumido pelo OrderConsumer");

                // --- Valida persistência ---
                Order? orderInDb = null;
                var dbTimeout = TimeSpan.FromSeconds(5);
                var dbStart = DateTime.UtcNow;

                while ((DateTime.UtcNow - dbStart) < dbTimeout)
                {
                    orderInDb = await dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == user.Id);
                    if (orderInDb != null) break;
                    await Task.Delay(100);
                }

                Assert.NotNull(orderInDb);
                Assert.Equal(user.Id, orderInDb!.UserId);

                // --- Verifica que EmailConsumer recebeu OrderProcessedEvent ---
                var processedConsumed = await harness.Consumed.Any<OrderProcessedEvent>();
                Assert.True(processedConsumed, "OrderProcessedEvent não foi publicado/consumido");

                // Opcional: verificar chamada do serviço de e-mail
                _factory.MockEmailService.Verify(e => e.SendOrderConfirmationAsync(orderInDb!.UserId, orderInDb.Id, orderInDb.TotalAmount, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Fact]
        public async Task Checkout_ShouldReturnBadRequest_WhenCartIsEmpty()
        {
            // limpa carrinho do usuário customer
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var user = await db.Users.FirstAsync(u => u.Email == "customer@test.com");
                var carts = db.Carts.Where(c => c.UserId == user.Id);
                db.Carts.RemoveRange(carts);
                await db.SaveChangesAsync();
            }

            var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = "customer@test.com", Password = "Password123!" });
            login.EnsureSuccessStatusCode();
            var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var req = new CreateOrderRequest
            {
                ShippingAddress = new CreateOrderAddressRequest { Street = "S", City = "C", State = "ST", PostalCode = "01001000" }
            };
            var resp = await _client.PostAsJsonAsync("/api/orders/checkout", req);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task CalculateShipping_ShouldReturnOk_WithMockedService()
        {
            var response = await _client.PostAsJsonAsync("/api/orders/calculate-shipping", new { DestinationZipCode = "20040030" });
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.True(root.TryGetProperty("shippingCost", out var sc));
            Assert.True(root.TryGetProperty("deliveryDays", out var dd));
            Assert.True(root.TryGetProperty("isRealApi", out var ir));
            Assert.True(root.TryGetProperty("originZipCode", out var oz));
            Assert.True(root.TryGetProperty("destinationZipCode", out var dz));
        }

        [Fact]
        public async Task CalculateShipping_ShouldReturnBadRequest_WhenServiceThrowsArgument()
        {
            // Força o mock a lançar ArgumentException
            _factory.MockShippingService
                .Setup(s => s.CalculateShippingWithDetailsFromStoreAsync(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Invalid zip"));

            var response = await _client.PostAsJsonAsync("/api/orders/calculate-shipping", new { DestinationZipCode = "invalid" });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
