namespace EcommerceNashApp.Core.Interfaces.IServices
{
    public interface IPaymentService
    {
        Task CreateOrUpdatePaymentIntentAsync(Guid userId);
    }
}
