namespace Ecommerce.API.Resources.Orders.DTOs.Responses
{
    public class ShippingCalculationResponse
    {
        public string OriginZipCode { get; set; } = string.Empty;
        public string DestinationZipCode { get; set; } = string.Empty;
        public decimal ShippingCost { get; set; }
        public string Currency { get; set; } = "BRL";
        public int DeliveryDays { get; set; }
        public string ServiceName { get; set; } = "SEDEX";
        public string ServiceCode { get; set; } = "04014";
        public ShippingAddressInfo OriginAddress { get; set; } = new();
        public ShippingAddressInfo DestinationAddress { get; set; } = new();
        public bool IsRealApi { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // Renamed to avoid ambiguity with Ecommerce.Application.Interfaces.AddressInfo
    public class ShippingAddressInfo
    {
        public string ZipCode { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string FullAddress => $"{Street}, {Neighborhood}, {City} - {State}, {ZipCode}";
    }
}



