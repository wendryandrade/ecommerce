using Ecommerce.Application.Interfaces.Infrastructure;
using System.Xml.Linq; 
using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Infrastructure.Services
{
    public class CorreiosShippingService : IShippingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _correiosApiUrl;

        public CorreiosShippingService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _correiosApiUrl = configuration["Correios:ApiUrl"] ?? "https://ws.correios.com.br/calculador/CalcPrecoPrazo.aspx";
        }

        public async Task<decimal> CalculateShippingCostAsync(string originZipCode, string destinationZipCode)
        {
            ArgumentException.ThrowIfNullOrEmpty(originZipCode);
            ArgumentException.ThrowIfNullOrEmpty(destinationZipCode);

            var client = _httpClientFactory.CreateClient();

            // Parâmetros para a consulta. "04014" é o código para SEDEX sem contrato.
            var queryParams = new Dictionary<string, string?>
            {
                { "nCdServico", "04014" },
                { "sCepOrigem", originZipCode.Replace("-", "") },
                { "sCepDestino", destinationZipCode.Replace("-", "") },
                { "nVlPeso", "1" }, // 1 kg (exemplo)
                { "nCdFormato", "1" }, // 1 = Caixa/Pacote
                { "nVlComprimento", "20" }, // cm
                { "nVlAltura", "10" },      // cm
                { "nVlLargura", "15" },     // cm
                { "nVlDiametro", "0" },
                { "sCdMaoPropria", "n" },
                { "nVlValorDeclarado", "0" },
                { "sCdAvisoRecebimento", "n" },
                { "nIndicaCalculo", "3" },
                { "out", "xml" },
                { "strRetorno", "xml" }
            };

            // Monta a URL final com os parâmetros
            var url = QueryHelpers.AddQueryString(_correiosApiUrl, queryParams);

            var responseString = await client.GetStringAsync(url);

            // --- A Mágica de Ler o XML ---
            var xml = XDocument.Parse(responseString);
            var servicoNode = xml.Descendants("cServico").FirstOrDefault();

            if (servicoNode != null)
            {
                // Verifica se houve erro retornado pela API dos Correios
                var erro = servicoNode.Element("Erro")?.Value;
                if (erro != null && erro != "0")
                {
                    var msgErro = servicoNode.Element("MsgErro")?.Value ?? "Erro desconhecido ao calcular o frete.";
                    throw new InvalidOperationException($"Erro da API dos Correios: {msgErro}");
                }

                var valorString = servicoNode.Element("Valor")?.Value;

                // Converte a string "123,45" para um decimal 123.45
                if (decimal.TryParse(valorString, NumberStyles.Any, new CultureInfo("pt-BR"), out var valorDecimal))
                {
                    return valorDecimal;
                }
            }

            // Se chegar aqui, algo deu errado com o XML ou o cálculo.
            throw new InvalidOperationException("Não foi possível calcular o frete. A resposta da API pode ter mudado.");
        }
    }
}