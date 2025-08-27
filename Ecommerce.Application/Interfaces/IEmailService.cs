namespace Ecommerce.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(Guid userId, Guid orderId, decimal totalAmount, CancellationToken cancellationToken = default);
    }
}
