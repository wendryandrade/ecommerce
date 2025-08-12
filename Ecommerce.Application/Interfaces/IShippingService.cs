namespace Ecommerce.Application.Interfaces.Infrastructure
{
    // Interface para um serviço que calcula frete.
    public interface IShippingService
    {
        // Retorna o custo do frete como um decimal.
        Task<decimal> CalculateShippingCostAsync(string originZipCode, string destinationZipCode);
    }
}