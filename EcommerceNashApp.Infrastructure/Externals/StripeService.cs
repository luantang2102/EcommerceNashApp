using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Stripe;

namespace EcommerceNashApp.Infrastructure.Externals
{
    public class StripeService
    {
        public StripeService(IOptions<StripeConfig> config)
        {
            StripeConfiguration.ApiKey = config.Value.StripeKey;
        }

        public async Task<PaymentIntent> CreateOrUpdatePaymentIntent(Cart cart)
        {
            var service = new PaymentIntentService();

            var intent = new PaymentIntent();
            var subtotal = cart.CartItems.Sum(x => x.Price * x.Quantity);
            var deliveryFee = 50000; // Change later

            if (string.IsNullOrEmpty(cart.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(subtotal + deliveryFee),
                    Currency = "vnd",
                    PaymentMethodTypes =
                    [
                        "card"
                    ]
                };
                intent = await service.CreateAsync(options);
            }
            else
            {
                var options = new PaymentIntentUpdateOptions
                {
                    Amount = (long)(subtotal + deliveryFee),
                };
                intent = await service.UpdateAsync(cart.PaymentIntentId, options);
            }
            return intent;
        }
    }
}
