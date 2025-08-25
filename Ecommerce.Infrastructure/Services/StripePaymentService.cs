using Ecommerce.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe; 

namespace Ecommerce.Infrastructure.Services
{
    public class StripePaymentService : IPaymentService
    {
        public StripePaymentService(IConfiguration configuration)
        {
            // Prefer environment variables or secrets over appsettings
            var apiKey = Environment.GetEnvironmentVariable("STRIPE_SECRETKEY")
                         ?? Environment.GetEnvironmentVariable("STRIPE__SECRETKEY")
                         ?? configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Stripe SecretKey is not configured. Set STRIPE_SECRETKEY env var or configuration 'Stripe:SecretKey'.");
            }
            StripeConfiguration.ApiKey = apiKey;
        }

        public async Task<string> ProcessPaymentAsync(decimal amount, string currency, string paymentMethodId)
        {
            try
            {
                var amountInCents = (long)(amount * 100);
                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = currency.ToLower(),
                    PaymentMethod = paymentMethodId,
                    Confirm = true,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                        AllowRedirects = "never"
                    }
                };

                var service = new PaymentIntentService();
                PaymentIntent paymentIntent = await service.CreateAsync(options);

                if (paymentIntent.Status == "succeeded")
                {
                    return paymentIntent.Id;
                }

                throw new InvalidOperationException($"O pagamento com Stripe não foi bem-sucedido. Status: {paymentIntent.Status}");

            }
            catch (StripeException e)
            {
                throw new InvalidOperationException($"Erro ao processar o pagamento com a API do Stripe: {e.Message}");
            }
        }
    }
}