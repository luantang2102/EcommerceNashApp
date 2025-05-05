using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
