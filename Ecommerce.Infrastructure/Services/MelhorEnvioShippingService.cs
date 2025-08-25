using Ecommerce.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Ecommerce.Infrastructure.Services.Exceptions;

namespace Ecommerce.Infrastructure.Services
{
    public class MelhorEnvioShippingService : IShippingService
    {
        private const int HttpTimeoutSeconds = 30;
        private const int DefaultDeliveryDays = 3;
        private const int MinZipDigitsForRegion = 3;
        private const int MockDelayMilliseconds = 200;
        private const decimal DefaultBasePrice = 15.00m;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MelhorEnvioShippingService> _logger;
        private readonly MelhorEnvioSettings _settings;

        public MelhorEnvioShippingService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<MelhorEnvioShippingService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settings = configuration.GetSection("MelhorEnvio").Get<MelhorEnvioSettings>() ?? new MelhorEnvioSettings();
        }

        public async Task<decimal> CalculateShippingCostAsync(string originZipCode, string destinationZipCode)
        {
            ArgumentException.ThrowIfNullOrEmpty(originZipCode);
            ArgumentException.ThrowIfNullOrEmpty(destinationZipCode);

            // Se UseMockData estiver habilitado, usa dados simulados
            if (_settings.UseMockData || string.IsNullOrWhiteSpace(_settings.ApiToken))
            {
                if (string.IsNullOrWhiteSpace(_settings.ApiToken))
                {
                    _logger.LogWarning("Token de API do Melhor Envio não configurado. Usando cálculo de frete simulado.");
                }
                _logger.LogInformation("Usando dados simulados para cálculo de frete via Melhor Envio");
                return await CalculateMockShippingAsync(originZipCode, destinationZipCode);
            }

            try
            {
                var (cost, _, _, _) = await CalculateRealShippingWithDetailsAsync(originZipCode, destinationZipCode);
                return cost;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao calcular frete real via Melhor Envio, usando simulação");
                return await CalculateMockShippingAsync(originZipCode, destinationZipCode);
            }
        }

        public async Task<(decimal cost, int deliveryDays, bool isRealApi, AddressInfo originAddress, AddressInfo destinationAddress)> CalculateShippingWithDetailsAsync(string originZipCode, string destinationZipCode)
        {
            ArgumentException.ThrowIfNullOrEmpty(originZipCode);
            ArgumentException.ThrowIfNullOrEmpty(destinationZipCode);

            // Se UseMockData estiver habilitado, usa dados simulados
            if (_settings.UseMockData || string.IsNullOrWhiteSpace(_settings.ApiToken))
            {
                if (string.IsNullOrWhiteSpace(_settings.ApiToken))
                {
                    _logger.LogWarning("Token de API do Melhor Envio não configurado. Usando cálculo de frete simulado.");
                }
                _logger.LogInformation("Usando dados simulados para cálculo de frete via Melhor Envio");
                var cost = await CalculateMockShippingAsync(originZipCode, destinationZipCode);
                var originAddress = await GetAddressFromViaCepAsync(originZipCode);
                var destinationAddress = await GetAddressFromViaCepAsync(destinationZipCode);
                return (cost, DefaultDeliveryDays, false, originAddress, destinationAddress);
            }

            try
            {
                var (cost, deliveryDays, originAddress, destinationAddress) = await CalculateRealShippingWithDetailsAsync(originZipCode, destinationZipCode);
                return (cost, deliveryDays, true, originAddress, destinationAddress);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao calcular frete real via Melhor Envio, usando simulação");
                var cost = await CalculateMockShippingAsync(originZipCode, destinationZipCode);
                var originAddress = await GetAddressFromViaCepAsync(originZipCode);
                var destinationAddress = await GetAddressFromViaCepAsync(destinationZipCode);
                return (cost, DefaultDeliveryDays, false, originAddress, destinationAddress);
            }
        }

        private async Task<(decimal cost, int deliveryDays, AddressInfo originAddress, AddressInfo destinationAddress)> CalculateRealShippingWithDetailsAsync(string originZipCode, string destinationZipCode)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

            // Remove caracteres não numéricos dos CEPs
            var origin = new string(originZipCode.Where(char.IsDigit).ToArray());
            var destination = new string(destinationZipCode.Where(char.IsDigit).ToArray());

            // Buscar informações dos endereços via ViaCEP
            var originAddress = await GetAddressFromViaCepAsync(origin);
            var destinationAddress = await GetAddressFromViaCepAsync(destination);

            // Monta o corpo da requisição para o Melhor Envio
            var requestBody = new
            {
                from = new { postal_code = origin },
                to = new { postal_code = destination },
                products = new[]
                {
                    new
                    {
                        id = "x",
                        width = _settings.DefaultPackageWidth,
                        height = _settings.DefaultPackageHeight,
                        length = _settings.DefaultPackageLength,
                        weight = _settings.DefaultPackageWeight,
                        insurance_value = _settings.DefaultInsuranceValue,
                        quantity = 1
                    }
                },
                options = new
                {
                    receipt = false,
                    own_hand = false
                },
                services = _settings.Services // "1,2,18" por exemplo
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiToken);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = _settings.ApiUrl;
            _logger.LogInformation("Consultando API do Melhor Envio: {Url}", url);

            try
            {
                using var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Resposta da API do Melhor Envio: {Json}", jsonContent);

                    using var document = JsonDocument.Parse(jsonContent);
                    var shippingOptions = document.RootElement;

                    if (shippingOptions.ValueKind == JsonValueKind.Array && shippingOptions.GetArrayLength() > 0)
                    {
                        // Pega a primeira opção de frete (normalmente a mais barata)
                        var firstOption = shippingOptions[0];

                        var cost = ParsePrice(firstOption);
                        var deliveryDays = ParseDeliveryDays(firstOption);
                        var serviceName = firstOption.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String
                            ? nameProp.GetString()
                            : string.Empty;

                        _logger.LogInformation("Frete calculado via Melhor Envio ({Service}): R$ {Cost}, Prazo: {Days} dias",
                            serviceName, cost, deliveryDays);

                        return (cost, deliveryDays, originAddress, destinationAddress);
                    }

                    _logger.LogWarning("Nenhuma opção de frete retornada pela API do Melhor Envio");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Resposta não-sucedida da API do Melhor Envio: {StatusCode} - {Content}",
                        response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar API do Melhor Envio");
                throw new ShippingCalculationException("Erro ao consultar API do Melhor Envio", ex);
            }

            // Se chegou até aqui, não conseguiu calcular o frete
            throw new ShippingCalculationException("Não foi possível calcular o frete via Melhor Envio");
        }

        private static decimal ParsePrice(JsonElement firstOption)
        {
            if (firstOption.TryGetProperty("price", out var priceProp))
            {
                switch (priceProp.ValueKind)
                {
                    case JsonValueKind.Number:
                        if (priceProp.TryGetDecimal(out var priceNum)) return priceNum;
                        break;
                    case JsonValueKind.String:
                        var str = priceProp.GetString();
                        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var priceStr))
                            return priceStr;
                        break;
                }
            }
            // Fallback
            return 0m;
        }

        private static int ParseDeliveryDays(JsonElement firstOption)
        {
            // Alguns retornos podem trazer delivery_time como número, string ou objeto
            if (firstOption.TryGetProperty("delivery_time", out var dtProp))
            {
                try
                {
                    return dtProp.ValueKind switch
                    {
                        JsonValueKind.Number => dtProp.GetInt32(),
                        JsonValueKind.String => int.TryParse(dtProp.GetString(), out var dts) ? dts : DefaultDeliveryDays,
                        JsonValueKind.Object =>
                            dtProp.TryGetProperty("days", out var daysProp) &&
                            ((daysProp.ValueKind == JsonValueKind.Number && daysProp.TryGetInt32(out var daysNum)) ||
                             (daysProp.ValueKind == JsonValueKind.String && int.TryParse(daysProp.GetString(), out daysNum)))
                                ? daysNum
                                : DefaultDeliveryDays,
                        _ => DefaultDeliveryDays
                    };
                }
                catch
                {
                    return DefaultDeliveryDays;
                }
            }
            return DefaultDeliveryDays;
        }

        private async Task<AddressInfo> GetAddressFromViaCepAsync(string zipCode)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                using var response = await client.GetAsync($"https://viacep.com.br/ws/{zipCode}/json/");

                if (!response.IsSuccessStatusCode)
                {
                    return CreateAddressFromZipCode(zipCode);
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(jsonResponse);
                var root = document.RootElement;

                if (root.TryGetProperty("erro", out _))
                {
                    return CreateAddressFromZipCode(zipCode);
                }

                return MapViaCepAddress(root, zipCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao buscar endereço via ViaCEP para CEP {ZipCode}", zipCode);
                return CreateAddressFromZipCode(zipCode);
            }
        }

        private static AddressInfo MapViaCepAddress(JsonElement root, string zipCode)
        {
            return new AddressInfo
            {
                ZipCode = zipCode,
                Street = root.TryGetProperty("logradouro", out var log) ? (log.GetString() ?? "Endereço não disponível") : "Endereço não disponível",
                Neighborhood = root.TryGetProperty("bairro", out var bai) ? (bai.GetString() ?? "Bairro não disponível") : "Bairro não disponível",
                City = root.TryGetProperty("localidade", out var loc) ? (loc.GetString() ?? "Cidade não encontrada") : "Cidade não encontrada",
                State = root.TryGetProperty("uf", out var uf) ? (uf.GetString() ?? "UF") : "UF"
            };
        }

        private static async Task<decimal> CalculateMockShippingAsync(string originZipCode, string destinationZipCode)
        {
            await Task.Delay(MockDelayMilliseconds); // Simula delay de rede

            var basePrice = DefaultBasePrice;
            var distanceFactor = CalculateDistanceFactor(originZipCode, destinationZipCode);

            return Math.Round(basePrice * distanceFactor, 2);
        }

        private static decimal CalculateDistanceFactor(string originZipCode, string destinationZipCode)
        {
            // Remove caracteres não numéricos
            var origin = new string(originZipCode.Where(char.IsDigit).ToArray());
            var dest = new string(destinationZipCode.Where(char.IsDigit).ToArray());

            if (origin.Length < MinZipDigitsForRegion || dest.Length < MinZipDigitsForRegion)
                return 1.0m; // fallback

            // Considera os 3 primeiros dígitos do CEP para "região"
            var originPrefixStr = origin.Substring(0, MinZipDigitsForRegion);
            var destPrefixStr = dest.Substring(0, MinZipDigitsForRegion);

            if (!int.TryParse(originPrefixStr, NumberStyles.None, CultureInfo.InvariantCulture, out var originPrefix) ||
                !int.TryParse(destPrefixStr, NumberStyles.None, CultureInfo.InvariantCulture, out var destPrefix))
            {
                return 1.0m;
            }

            var diff = Math.Abs(originPrefix - destPrefix);

            return diff switch
            {
                0 => 1.0m,        // mesma região
                <= 50 => 1.5m,    // região próxima
                <= 200 => 1.8m,   // intermediária
                _ => 2.5m         // distante
            };
        }

        private static AddressInfo CreateAddressFromZipCode(string zipCode)
        {
            // Mapeamento básico de CEPs para cidades/estados
            var cepInfo = GetCepInfo(zipCode);

            return new AddressInfo
            {
                ZipCode = zipCode,
                Street = "Endereço não disponível",
                Neighborhood = "Bairro não disponível",
                City = cepInfo.city,
                State = cepInfo.state
            };
        }

        private static readonly Dictionary<string, (string, string)> s_cepPrefixMap = new Dictionary<string, (string, string)>
        {
            ["010"] = ("São Paulo", "SP"),
            ["013"] = ("São Paulo", "SP"),
            ["020"] = ("São Paulo", "SP"),
            ["030"] = ("São Paulo", "SP"),
            ["040"] = ("São Paulo", "SP"),
            ["050"] = ("São Paulo", "SP"),
            ["060"] = ("Osasco", "SP"),
            ["070"] = ("Guarulhos", "SP"),
            ["080"] = ("São Paulo", "SP"),
            ["090"] = ("Santo André", "SP"),
            ["100"] = ("São Paulo", "SP"),
            ["110"] = ("Santos", "SP"),
            ["120"] = ("São José dos Campos", "SP"),
            ["130"] = ("Campinas", "SP"),
            ["140"] = ("Ribeirão Preto", "SP"),
            ["150"] = ("São José do Rio Preto", "SP"),
            ["160"] = ("Araçatuba", "SP"),
            ["170"] = ("Bauru", "SP"),
            ["180"] = ("Sorocaba", "SP"),
            ["190"] = ("Presidente Prudente", "SP"),
            ["200"] = ("Rio de Janeiro", "RJ"),
            ["210"] = ("Rio de Janeiro", "RJ"),
            ["220"] = ("Rio de Janeiro", "RJ"),
            ["230"] = ("Rio de Janeiro", "RJ"),
            ["240"] = ("Niterói", "RJ"),
            ["250"] = ("Duque de Caxias", "RJ"),
            ["260"] = ("Nova Iguaçu", "RJ"),
            ["270"] = ("Volta Redonda", "RJ"),
            ["280"] = ("Campos dos Goytacazes", "RJ"),
            ["290"] = ("Vitória", "ES"),
            ["300"] = ("Belo Horizonte", "MG"),
            ["310"] = ("Belo Horizonte", "MG"),
            ["320"] = ("Contagem", "MG"),
            ["324"] = ("Belo Horizonte", "MG"),
            ["330"] = ("Betim", "MG"),
            ["340"] = ("Nova Lima", "MG"),
            ["350"] = ("Governador Valadares", "MG"),
            ["360"] = ("Juiz de Fora", "MG"),
            ["370"] = ("Varginha", "MG"),
            ["380"] = ("Uberaba", "MG"),
            ["390"] = ("Uberlândia", "MG"),
            ["400"] = ("Salvador", "BA"),
            ["410"] = ("Salvador", "BA"),
            ["420"] = ("Lauro de Freitas", "BA"),
            ["430"] = ("Camaçari", "BA"),
            ["440"] = ("Feira de Santana", "BA"),
            ["450"] = ("Vitória da Conquista", "BA"),
            ["460"] = ("Ilhéus", "BA"),
            ["470"] = ("Juazeiro", "BA"),
            ["480"] = ("Alagoinhas", "BA"),
            ["490"] = ("Aracaju", "SE"),
            ["500"] = ("Recife", "PE"),
            ["510"] = ("Recife", "PE"),
            ["520"] = ("Recife", "PE"),
            ["530"] = ("Olinda", "PE"),
            ["540"] = ("Jaboatão dos Guararapes", "PE"),
            ["550"] = ("Caruaru", "PE"),
            ["560"] = ("Petrolina", "PE"),
            ["570"] = ("Maceió", "AL"),
            ["580"] = ("João Pessoa", "PB"),
            ["590"] = ("Natal", "RN"),
            ["600"] = ("Fortaleza", "CE"),
            ["610"] = ("Fortaleza", "CE"),
            ["620"] = ("Sobral", "CE"),
            ["630"] = ("Juazeiro do Norte", "CE"),
            ["640"] = ("Teresina", "PI"),
            ["650"] = ("São Luís", "MA"),
            ["660"] = ("Belém", "PA"),
            ["670"] = ("Ananindeua", "PA"),
            ["680"] = ("Macapá", "AP"),
            ["690"] = ("Manaus", "AM"),
            ["700"] = ("Brasília", "DF"),
            ["710"] = ("Brasília", "DF"),
            ["720"] = ("Brasília", "DF"),
            ["730"] = ("Brasília", "DF"),
            ["740"] = ("Goiânia", "GO"),
            ["750"] = ("Aparecida de Goiânia", "GO"),
            ["760"] = ("Anápolis", "GO"),
            ["770"] = ("Palmas", "TO"),
            ["780"] = ("Cuiabá", "MT"),
            ["790"] = ("Campo Grande", "MS"),
            ["800"] = ("Curitiba", "PR"),
            ["810"] = ("Curitiba", "PR"),
            ["820"] = ("Curitiba", "PR"),
            ["830"] = ("São José dos Pinhais", "PR"),
            ["840"] = ("Ponta Grossa", "PR"),
            ["850"] = ("Guarapuava", "PR"),
            ["860"] = ("Londrina", "PR"),
            ["870"] = ("Maringá", "PR"),
            ["880"] = ("Florianópolis", "SC"),
            ["890"] = ("Blumenau", "SC"),
            ["900"] = ("Porto Alegre", "RS"),
            ["910"] = ("Porto Alegre", "RS"),
            ["920"] = ("Canoas", "RS"),
            ["930"] = ("São Leopoldo", "RS"),
            ["940"] = ("Gravataí", "RS"),
            ["950"] = ("Caxias do Sul", "RS"),
            ["960"] = ("Pelotas", "RS"),
            ["970"] = ("Santa Maria", "RS"),
            ["980"] = ("Passo Fundo", "RS"),
            ["990"] = ("Erechim", "RS"),
        };

        private static (string city, string state) GetCepInfo(string zipCode)
        {
            var cep = new string(zipCode.Where(char.IsDigit).ToArray());

            if (cep.Length < MinZipDigitsForRegion) return ("Cidade não encontrada", "UF");

            var prefix = cep.Substring(0, MinZipDigitsForRegion);

            return s_cepPrefixMap.TryGetValue(prefix, out var result)
                ? result
                : ("Cidade não encontrada", "UF");
        }

        public string GetStoreZipCode()
        {
            return _settings.DefaultOriginZipCode;
        }

        public async Task<decimal> CalculateShippingCostFromStoreAsync(string destinationZipCode)
        {
            var storeZipCode = GetStoreZipCode();
            return await CalculateShippingCostAsync(storeZipCode, destinationZipCode);
        }

        public async Task<(decimal cost, int deliveryDays, bool isRealApi, AddressInfo originAddress, AddressInfo destinationAddress)> CalculateShippingWithDetailsFromStoreAsync(string destinationZipCode)
        {
            var storeZipCode = GetStoreZipCode();
            return await CalculateShippingWithDetailsAsync(storeZipCode, destinationZipCode);
        }
    }

    public class MelhorEnvioSettings
    {
        public string ApiUrl { get; set; } = "https://sandbox.melhorenvio.com.br/api/v2/me/shipment/calculate";
        public string ApiToken { get; set; } = string.Empty; // Configure via appsettings ou variáveis de ambiente
        public string DefaultOriginZipCode { get; set; } = "01001000";
        public string Services { get; set; } = "1,2,18"; // PAC, SEDEX, etc
        public bool UseMockData { get; set; } = false;

        // Configurações padrão do pacote
        public int DefaultPackageWidth { get; set; } = 10;
        public int DefaultPackageHeight { get; set; } = 10;
        public int DefaultPackageLength { get; set; } = 10;
        public decimal DefaultPackageWeight { get; set; } = 1;
        public decimal DefaultInsuranceValue { get; set; } = 0;
    }
}