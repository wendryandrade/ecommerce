using Ecommerce.API.Resources.Auth.DTOs.Requests;
using Ecommerce.API.Resources.Auth.DTOs.Responses;
using Ecommerce.API.Resources.Users.DTOs.Responses;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Ecommerce.API.IntegrationTests.Resources.Users.Controllers
{
    public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UsersControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        // Deveria retornar Unauthorized (401) quando a requisição não tem token
        public async Task GetById_ShouldReturnUnauthorized_WhenNoTokenIsProvided()
        {
            // Act
            var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        // Deveria retornar o usuário quando ele existe e tem token
        public async Task GetByEmail_ShouldReturnUser_WhenUserExistsAndTokenIsProvided()
        {
            // Arrange: Faz login com o usuário Customer para obter um token
            var loginRequest = new LoginRequest { Email = "customer@test.com", Password = "Password123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/users/by-email?email=customer@test.com");

            // Assert
            response.EnsureSuccessStatusCode();
            var userResponse = await response.Content.ReadFromJsonAsync<UserResponse>();
            Assert.Equal("customer@test.com", userResponse!.Email);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WhenAdmin()
        {
            var loginRequest = new LoginRequest { Email = "admin@test.com", Password = "Password123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/users");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetAll_ShouldReturnForbidden_WhenCustomer()
        {
            var loginRequest = new LoginRequest { Email = "customer@test.com", Password = "Password123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenUserExists()
        {
            var loginRequest = new LoginRequest { Email = "customer@test.com", Password = "Password123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var userId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var response = await _client.GetAsync($"/api/users/{userId}");
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<UserResponse>();
            Assert.Equal("customer@test.com", user!.Email);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var loginRequest = new LoginRequest { Email = "admin@test.com", Password = "Password123!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var token = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}