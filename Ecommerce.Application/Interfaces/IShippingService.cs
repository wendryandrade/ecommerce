namespace Ecommerce.Application.Interfaces
{
    // Interface para um serviço que calcula frete.
    public interface IShippingService
    {
        // Retorna o custo do frete como um decimal.
        Task<decimal> CalculateShippingCostAsync(string originZipCode, string destinationZipCode);
        
        // Retorna o custo do frete com detalhes (prazo, se é API real e endereços)
        Task<(decimal cost, int deliveryDays, bool isRealApi, AddressInfo originAddress, AddressInfo destinationAddress)> CalculateShippingWithDetailsAsync(string originZipCode, string destinationZipCode);
        
        // Novos métodos que usam o CEP fixo da loja
        Task<decimal> CalculateShippingCostFromStoreAsync(string destinationZipCode);
        
        // Retorna o custo do frete com detalhes usando o CEP fixo da loja
        Task<(decimal cost, int deliveryDays, bool isRealApi, AddressInfo originAddress, AddressInfo destinationAddress)> CalculateShippingWithDetailsFromStoreAsync(string destinationZipCode);
        
        // Retorna o CEP padrão da loja
        string GetStoreZipCode();
    }

    public class AddressInfo
    {
        public string ZipCode { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string FullAddress => $"{Street}, {Neighborhood}, {City} - {State}, {ZipCode}";
    }
}