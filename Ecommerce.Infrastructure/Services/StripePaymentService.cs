using Ecommerce.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe; 

namespace Ecommerce.Infrastructure.Services
{
    public class StripePaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;

        public StripePaymentService(IConfiguration configuration)
        {
            _configuration = configuration;

            // A chave da API é configurada uma vez para toda a aplicação.
            // O .NET vai buscar em appsettings.json, appsettings.Development.json e, finalmente, no User Secrets.
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<string> ProcessPaymentAsync(decimal amount, string currency, string paymentMethodId)
        {
            try
            {
                // O Stripe trabalha com o menor valor da moeda (centavos).
                var amountInCents = (long)(amount * 100);

                // Opções para a criação da "Intenção de Pagamento"
                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = currency.ToLower(),
                    PaymentMethod = paymentMethodId,
                    ConfirmationMethod = "manual", 
                    Confirm = true, // Tenta capturar o pagamento assim que o PaymentIntent é criado
                };

                var service = new PaymentIntentService();
                PaymentIntent paymentIntent = await service.CreateAsync(options);

                // Após a confirmação, verificamos o status.
                // "succeeded" significa que o dinheiro foi capturado com sucesso.
                if (paymentIntent.Status == "succeeded")
                {
                    // Retornamos o ID da transação, que é a prova do pagamento.
                    return paymentIntent.Id;
                }

                // Se o status for qualquer outra coisa (ex: "requires_action"), o pagamento não foi concluído.
                // Lançamos uma exceção para que o fluxo de pedido seja interrompido.
                throw new InvalidOperationException($"O pagamento com Stripe não foi bem-sucedido. Status: {paymentIntent.Status}");

            }
            catch (StripeException e)
            {
                // Captura erros específicos da API do Stripe (ex: cartão recusado, chave inválida)
                // e fornece uma mensagem de erro clara.
                throw new InvalidOperationException($"Erro ao processar o pagamento com a API do Stripe: {e.Message}");
            }
        }
    }
}