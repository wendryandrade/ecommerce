using Ecommerce.API.Resources.Auth.DTOs.Requests;
using Ecommerce.API.Resources.Auth.DTOs.Responses;
using Ecommerce.API.Resources.Users.DTOs.Requests;
using System.Net;
using System.Net.Http.Json;

namespace Ecommerce.API.IntegrationTests.Resources.Auth.Controllers
{

    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        // Deveria retornar 200 OK quando o usuário é registrado com sucesso
        public async Task Register_ShouldReturnOk_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange: Crie um usuário com uma DTO de registro sem a Role
            var request = new CreateUserRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "new.register@email.com",
                Password = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        // Deveria retornar o token JWT quando as credenciais são válidas
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                // Se a resposta falhar, leia o conteúdo e jogue no output do teste
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.Fail($"A requisição falhou com o status {response.StatusCode}. Conteúdo do erro: {errorContent}");
            }

            // Se a resposta for bem-sucedida, continue com os asserts originais
            response.EnsureSuccessStatusCode();
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(loginResponse?.Token);
            Assert.NotEmpty(loginResponse.Token);
        }
    }
}