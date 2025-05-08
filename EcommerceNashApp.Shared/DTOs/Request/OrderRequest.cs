using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Shared.DTOs.Request
{
    public class OrderRequest
    {
        public bool SaveAddress { get; set; }
        public ShippingAddressRequest ShippingAddress { get; set; } = null!;
    }
}
