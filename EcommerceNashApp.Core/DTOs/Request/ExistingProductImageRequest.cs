using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.DTOs.Request
{
    public class ExistingProductImageRequest
    {
        public required string ImageUrl { get; set; }
        public bool IsMain { get; set; }
    }
}
