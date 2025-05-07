using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Services.External;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ICartRepository _cartRepository;
        private readonly StripeService stripeService;

        public PaymentService(ICartRepository cartRepository, StripeService stripeService)
        {
            _cartRepository = cartRepository;
            this.stripeService = stripeService;
        }

        public async Task<string?> CreateOrUpdatePaymentIntentAsync(Guid userId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "userId", userId }
                };
                throw new AppException(ErrorCode.CART_NOT_FOUND, attributes);
            }

            if (cart.CartItems.Count == 0)
            {
                cart.PaymentIntentId = null;
                cart.ClientSecret = null;
                await _cartRepository.UpdateAsync(cart);

                return null;
            }

            var paymentIntent = await stripeService.CreateOrUpdatePaymentIntent(cart);
            cart.PaymentIntentId = paymentIntent.Id;
            cart.ClientSecret = paymentIntent.ClientSecret;
            await _cartRepository.UpdateAsync(cart);

            return cart.ClientSecret;
        }
    }
}