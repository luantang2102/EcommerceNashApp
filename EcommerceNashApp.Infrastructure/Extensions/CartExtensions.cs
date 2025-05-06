using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Shared.DTOs.Response;

namespace EcommerceNashApp.Infrastructure.Extensions
{
    public static class CartExtensions
    {
        public static CartResponse MapModelToResponse(this Cart cart)
        {
            return new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartItems = cart.CartItems.Select(x => x.MapModelToResponse()).ToList(),
                PaymentIntentId = cart.PaymentIntentId,
                ClientSecret = cart.ClientSecret
            };
        }
    }
}
