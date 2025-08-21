using System;
using System.Threading.Tasks;
using Ecommerce.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Ecommerce.Infrastructure.UnitTests.Services
{
	public class StripePaymentServiceTests
	{
		private static IConfiguration CreateConfig(string? secretKey)
		{
			var dict = new Dictionary<string, string?>
			{
				{"Stripe:SecretKey", secretKey}
			};
			return new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
		}

		[Fact]
		public async Task ProcessPaymentAsync_WithInvalidKey_Throws()
		{
			var config = CreateConfig("sk_test_invalid");
			var service = new StripePaymentService(config);
			await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessPaymentAsync(10m, "BRL", "pm_card_visa"));
		}
	}
}


