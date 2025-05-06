namespace EcommerceNashApp.Web.Services
{
    public interface IPaymentService
    {
        Task<string> CreateOrUpdatePaymentIntentAsync();
    }
}