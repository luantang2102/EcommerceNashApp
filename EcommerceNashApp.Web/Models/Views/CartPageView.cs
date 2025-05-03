using EcommerceNashApp.Web.Models.DTOs;
using System.Collections.Generic;

namespace EcommerceNashApp.Web.Models.Views
{
    public class CartPageView
    {
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public double Subtotal => Items.Sum(item => item.Price * item.Quantity);
    }
}