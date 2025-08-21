using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace Ecommerce.Infrastructure.UnitTests.Services
{
	public class CorreiosShippingServiceTests
	{
		private static IConfiguration CreateConfig(string? url = null)
		{
			var dict = new Dictionary<string, string?>
			{
				{"Correios:ApiUrl", url}
			};
			return new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
		}

		[Fact]
		public async Task CalculateShippingCostAsync_Parses_Xml_And_Returns_Value()
		{
			var xml = """
			<Servicos>
				<cServico>
					<Valor>23,45</Valor>
					<Erro>0</Erro>
				</cServico>
			</Servicos>
			""";

			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(xml)
				});

			var httpClient = new HttpClient(handlerMock.Object);
			var factory = new Mock<IHttpClientFactory>();
			factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

			var service = new CorreiosShippingService(factory.Object, CreateConfig("http://fake"));
			var value = await service.CalculateShippingCostAsync("01001-000", "20040-000");
			Assert.Equal(23.45m, value);
		}

		[Fact]
		public async Task CalculateShippingCostAsync_ErroNode_Throws()
		{
			var xml = """
			<Servicos>
				<cServico>
					<Valor>0,00</Valor>
					<Erro>1</Erro>
					<MsgErro>CEP inválido</MsgErro>
				</cServico>
			</Servicos>
			""";

			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(xml)
				});

			var httpClient = new HttpClient(handlerMock.Object);
			var factory = new Mock<IHttpClientFactory>();
			factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

			var service = new CorreiosShippingService(factory.Object, CreateConfig("http://fake"));
			await Assert.ThrowsAsync<InvalidOperationException>(() => service.CalculateShippingCostAsync("01001-000", "20040-000"));
		}
	}
}


