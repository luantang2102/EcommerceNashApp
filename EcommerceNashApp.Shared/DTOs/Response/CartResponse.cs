using System;
using System.Collections.Generic;

namespace EcommerceNashApp.Shared.DTOs.Response
{
    public class CartResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<CartItemResponse> CartItems { get; set; } = new List<CartItemResponse>();
        public string? PaymentIntentId { get; set; }
        public string? ClientSecret { get; set; }
    }
}
