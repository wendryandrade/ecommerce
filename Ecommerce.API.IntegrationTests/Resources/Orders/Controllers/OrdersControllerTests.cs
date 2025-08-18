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

            // Configura os mocks do consumer
            _factory.MockShippingService
                .Setup(s => s.CalculateShippingCostAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(0m);

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

                var response = await _client.PostAsJsonAsync("/api/orders/checkout", createOrderRequest);
                response.EnsureSuccessStatusCode();

                // --- Aguarda consumo do evento ---
                var consumerHarness = _factory.Services.GetRequiredService<IConsumerTestHarness<OrderConsumer>>();
                var consumed = await consumerHarness.Consumed.Any<OrderSubmissionEvent>();
                Assert.True(consumed, "O evento OrderSubmissionEvent não foi consumido pelo OrderConsumer");

                // --- Valida persistência ---
                Order? orderInDb = null;
                var timeout = TimeSpan.FromSeconds(5);
                var start = DateTime.UtcNow;

                while ((DateTime.UtcNow - start) < timeout)
                {
                    orderInDb = await dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == user.Id);
                    if (orderInDb != null) break;
                    await Task.Delay(100);
                }

                Assert.NotNull(orderInDb);
                Assert.Equal(user.Id, orderInDb!.UserId);
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}
