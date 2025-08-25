using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using Ecommerce.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Ecommerce.Infrastructure.UnitTests.Services
{
    public class MelhorEnvioShippingServiceTests
    {
        private static IConfiguration CreateConfig(string? url = null)
        {
            var dict = new Dictionary<string, string?>
            {
                {"Correios:ApiUrl", url}
            };
            return new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        }

        private static ILogger<MelhorEnvioShippingService> CreateLogger()
        {
            var loggerMock = new Mock<ILogger<MelhorEnvioShippingService>>();
            return loggerMock.Object;
        }

        [Fact]
        public async Task CalculateShippingCostAsync_Returns_Value()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("""{""cep"":""01001-000"",""logradouro"":""Praça da Sé"",""bairro"":""Sé"",""localidade"":""São Paulo"",""uf"":""SP"",""ddd"":""11""}""")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var service = new MelhorEnvioShippingService(factory.Object, CreateConfig("http://fake"), CreateLogger());
            var value = await service.CalculateShippingCostAsync("01001-000", "20040-000");
            Assert.True(value > 0); // O valor depende do cálculo simulado
        }

        [Fact]
        public async Task CalculateShippingCostAsync_With_Invalid_Cep_Returns_Value()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("""{""erro"":true}""")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var service = new MelhorEnvioShippingService(factory.Object, CreateConfig("http://fake"), CreateLogger());
            var value = await service.CalculateShippingCostAsync("01001-000", "20040-000");
            Assert.True(value > 0); // Deve retornar valor simulado mesmo com CEP inválido
        }

        [Fact]
        public async Task CalculateShippingCostAsync_UsesMock_WhenNoToken_SameRegion()
        {
            // Arrange
            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["MelhorEnvio:ApiToken"] = string.Empty,
                ["MelhorEnvio:UseMockData"] = "true"
            });
            var service = CreateService(config, new RoutingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));

            // Same prefix => factor 1.0
            var origin = "01001000";
            var destination = "01015000";

            // Act
            var cost = await service.CalculateShippingCostAsync(origin, destination);

            // Assert
            Assert.Equal(15.00m, cost);
        }

        [Theory]
        [InlineData("01001000", "04000000", 22.50)] // diff 30 => 1.5 factor
        [InlineData("01001000", "20000000", 27.00)] // diff 190 => 1.8 factor
        [InlineData("01001000", "40000000", 37.50)] // diff 390 => 2.5 factor
        public async Task CalculateShippingCostAsync_MockDistanceFactor_Tiers(string origin, string destination, decimal expected)
        {
            // Arrange
            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["MelhorEnvio:ApiToken"] = string.Empty,
                ["MelhorEnvio:UseMockData"] = "true"
            });
            var service = CreateService(config, new RoutingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));

            // Act
            var cost = await service.CalculateShippingCostAsync(origin, destination);

            // Assert
            Assert.Equal(expected, cost);
        }

        [Fact]
        public async Task CalculateShippingWithDetailsAsync_UsesViaCep_ForAddresses_InMockPath()
        {
            // Arrange
            var originZip = "01001000";
            var destZip = "01310930";

            var handler = new RoutingHttpMessageHandler(req =>
            {
                if (req.RequestUri!.Host.Contains("viacep.com.br"))
                {
                    var zip = req.RequestUri!.Segments[^2].TrimEnd('/');
                    var json = JsonSerializer.Serialize(new
                    {
                        logradouro = zip == originZip ? "Praça da Sé" : "Avenida Paulista",
                        bairro = zip == originZip ? "Sé" : "Bela Vista",
                        localidade = "São Paulo",
                        uf = "SP"
                    });
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["MelhorEnvio:ApiToken"] = string.Empty,
                ["MelhorEnvio:UseMockData"] = "true"
            });
            var service = CreateService(config, handler);

            // Act
            var (cost, days, isReal, originAddr, destAddr) = await service.CalculateShippingWithDetailsAsync(originZip, destZip);

            // Assert
            Assert.False(isReal);
            Assert.Equal(3, days);
            Assert.Equal(originZip, originAddr.ZipCode);
            Assert.Equal("Praça da Sé", originAddr.Street);
            Assert.Equal(destZip, destAddr.ZipCode);
            Assert.Equal("Avenida Paulista", destAddr.Street);
            Assert.True(cost > 0);
        }

        [Fact]
        public async Task CalculateShippingWithDetailsAsync_RealApi_ReturnsCostAndDays()
        {
            // Arrange
            var apiUrl = "https://api.test/melhorenvio/calc";
            var originZip = "01001000";
            var destZip = "20040030";

            var handler = new RoutingHttpMessageHandler(req =>
            {
                if (req.RequestUri!.Host.Contains("viacep.com.br"))
                {
                    var json = JsonSerializer.Serialize(new
                    {
                        logradouro = "Rua X",
                        bairro = "Centro",
                        localidade = req.RequestUri!.ToString().Contains(originZip) ? "São Paulo" : "Rio de Janeiro",
                        uf = req.RequestUri!.ToString().Contains(originZip) ? "SP" : "RJ"
                    });
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                }
                if (req.Method == HttpMethod.Post && req.RequestUri!.ToString() == apiUrl)
                {
                    var options = new[]
                    {
                        new
                        {
                            price = 12.34m,
                            delivery_time = new { days = 5 },
                            name = "SEDEX"
                        }
                    };
                    var json = JsonSerializer.Serialize(options);
                    var resp = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                    resp.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
                    return resp;
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["MelhorEnvio:ApiToken"] = "token",
                ["MelhorEnvio:UseMockData"] = "false",
                ["MelhorEnvio:ApiUrl"] = apiUrl,
                ["MelhorEnvio:Services"] = "1,2,18"
            });
            var service = CreateService(config, handler);

            // Act
            var (cost, days, isReal, originAddr, destAddr) = await service.CalculateShippingWithDetailsAsync(originZip, destZip);

            // Assert
            Assert.True(isReal);
            Assert.Equal(5, days);
            Assert.Equal(12.34m, cost);
            Assert.Equal("SP", originAddr.State);
            Assert.Equal("RJ", destAddr.State);
        }

        // Additional tests moved from MelhorEnvioShippingServiceAdditionalTests
        [Theory]
        [InlineData("7", null, 7)]
        [InlineData(null, 8, 8)]
        public async Task CalculateShippingWithDetailsAsync_ParsesDeliveryDays_PrimitiveShapes(string? daysString, int? daysNumber, int expected)
        {
            var originZip = "01001000";
            var destZip = "20040030";
            var apiUrl = "https://api.test/melhorenvio/calc";

            var handler = new RoutingHttpMessageHandler(req =>
            {
                if (req.RequestUri!.Host.Contains("viacep.com.br"))
                {
                    var json = JsonSerializer.Serialize(new { logradouro = "X", bairro = "Y", localidade = "Z", uf = "UF" });
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                }

                if (req.Method == HttpMethod.Post && req.RequestUri!.ToString() == apiUrl)
                {
                    object delivery = daysString is not null ? (object)daysString : daysNumber!.Value;
                    var payload = new[] { new { price = 10.0m, delivery_time = delivery, name = "X" } };
                    var json = JsonSerializer.Serialize(payload);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["MelhorEnvio:ApiToken"] = "token",
                ["MelhorEnvio:ApiUrl"] = apiUrl,
                ["MelhorEnvio:UseMockData"] = "false",
                ["MelhorEnvio:Services"] = "1,2,18"
            });

            var service = CreateService(config, handler);

            var (_, days, isReal, _, _) = await service.CalculateShippingWithDetailsAsync(originZip, destZip);

            Assert.True(isReal);
            Assert.Equal(expected, days);
        }

        [Fact]
        public async Task CalculateShippingWithDetailsAsync_ParsesDeliveryDays_FromObjectShape()
        {
            var originZip = "01001000";
            var destZip = "20040030";
            var apiUrl = "https://api.test/melhorenvio/calc";

            var handler = new RoutingHttpMessageHandler(req =>
            {
                if (req.RequestUri!.Host.Contains("viacep.com.br"))
                {
                    var json = JsonSerializer.Serialize(new { logradouro = "X", bairro = "Y", localidade = "Z", uf = "UF" });
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                }
                if (req.Method == HttpMethod.Post && req.RequestUri!.ToString() == apiUrl)
                {
                    var payload = new[] { new { price = 10.0m, delivery_time = new { days = "9" }, name = "X" } };
                    var json = JsonSerializer.Serialize(payload);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["MelhorEnvio:ApiToken"] = "token",
                ["MelhorEnvio:ApiUrl"] = apiUrl,
                ["MelhorEnvio:UseMockData"] = "false",
                ["MelhorEnvio:Services"] = "1,2,18"
            });

            var service = CreateService(config, handler);

            var (_, days, isReal, _, _) = await service.CalculateShippingWithDetailsAsync(originZip, destZip);

            Assert.True(isReal);
            Assert.Equal(9, days);
        }

        [Fact]
        public async Task CalculateShippingWithDetailsAsync_FallsBack_OnApiError()
        {
            var apiUrl = "https://api.test/melhorenvio/calc";
            var originZip = "01001000";
            var destZip = "20040030";

            var handler = new RoutingHttpMessageHandler(req =>
            {
                if (req.RequestUri!.Host.Contains("viacep.com.br"))
                {
                    var json = JsonSerializer.Serialize(new { logradouro = "X", bairro = "Y", localidade = "Z", uf = "UF" });
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                }
                if (req.Method == HttpMethod.Post && req.RequestUri!.ToString() == apiUrl)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["MelhorEnvio:ApiToken"] = "token",
                ["MelhorEnvio:ApiUrl"] = apiUrl,
                ["MelhorEnvio:UseMockData"] = "false",
                ["MelhorEnvio:Services"] = "1,2,18"
            });

            var service = CreateService(config, handler);

            var (_, days, isReal, _, _) = await service.CalculateShippingWithDetailsAsync(originZip, destZip);

            Assert.False(isReal);
            Assert.Equal(3, days);
        }

        [Fact]
        public async Task CalculateShippingWithDetailsAsync_FallsBackAddress_OnViaCepFailure()
        {
            var originZip = "01001000";
            var destZip = "01310930";

            var handler = new RoutingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["MelhorEnvio:ApiToken"] = string.Empty,
                ["MelhorEnvio:UseMockData"] = "true"
            });

            var service = CreateService(config, handler);

            var (_, days, isReal, originAddr, destAddr) = await service.CalculateShippingWithDetailsAsync(originZip, destZip);

            Assert.False(isReal);
            Assert.Equal(3, days);
            Assert.Equal("São Paulo", originAddr.City);
            Assert.Equal("SP", originAddr.State);
            Assert.Equal("São Paulo", destAddr.City);
            Assert.Equal("SP", destAddr.State);
        }

        [Fact]
        public async Task CalculateShippingCostFromStoreAsync_UsesDefaultOrigin()
        {
            var config = BuildConfig(new Dictionary<string, string?>
            {
                ["MelhorEnvio:ApiToken"] = string.Empty,
                ["MelhorEnvio:UseMockData"] = "true",
                ["MelhorEnvio:DefaultOriginZipCode"] = "01001000"
            });
            var service = CreateService(config, new RoutingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
            var cost = await service.CalculateShippingCostFromStoreAsync("01310930");
            Assert.Equal(22.50m, cost);
        }

        private static IConfiguration BuildConfig(IDictionary<string, string?> values)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(values!)
                .Build();
        }

        private static MelhorEnvioShippingService CreateService(IConfiguration configuration, HttpMessageHandler handler)
        {
            var factory = new TestHttpClientFactory(handler);
            var logger = NullLogger<MelhorEnvioShippingService>.Instance;
            return new MelhorEnvioShippingService(factory, configuration, logger);
        }

        private sealed class TestHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _client;
            public TestHttpClientFactory(HttpMessageHandler handler)
            {
                _client = new HttpClient(handler, disposeHandler: true);
                _client.Timeout = TimeSpan.FromSeconds(10);
            }
            public HttpClient CreateClient(string name) => _client;
        }

        private sealed class RoutingHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
            public RoutingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            {
                _responder = responder;
            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_responder(request));
            }
        }
    }
}


