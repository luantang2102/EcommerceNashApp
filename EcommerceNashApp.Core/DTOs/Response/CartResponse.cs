using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.DTOs.Response
{
    public class CartResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<CartItemResponse> CartItems { get; set; } = [];
        public string? PaymentIntentId { get; set; }
        public string? ClientSecret { get; set; }
    }
}
