namespace EcommerceNashApp.Core.Interfaces.IServices
{
    public interface IPaymentService
    {
        Task<string?> CreateOrUpdatePaymentIntentAsync(Guid userId);
    }
}
