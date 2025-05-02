using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Infrastructure.Services
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
