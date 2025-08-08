using Ecommerce.API.Resources.Auth.DTOs.Requests;
using Ecommerce.API.Resources.Auth.DTOs.Responses;
using Ecommerce.API.Resources.Categories.DTOs.Requests;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Ecommerce.API.IntegrationTests.Resources.Categories.Controllers
{
    public class CategoriesControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CategoriesControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        // Deveria retornar todas as categorias com status OK
        public async Task GetAll_Categories_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/categories");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        // Deveria retornar Created (201) quando um usuário Admin cria uma nova categoria
        public async Task Create_Category_ShouldReturnCreated_WhenUserIsAdmin()
        {
            // Arrange: Faz login com o usuário Admin para obter um token
            var loginRequest = new LoginRequest { Email = "admin@test.com", Password = "Password123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createRequest = new CreateCategoryRequest
            {
                Name = "Test Category",
                Description = "A category for testing."
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/categories", createRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }
}