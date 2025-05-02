using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.DTOs.Request
{
    public class CartItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
