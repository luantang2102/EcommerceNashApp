using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.DTOs.Response
{
    public class ProductImageResponse
    {
        public Guid Id { get; set; }
        public required string ImageUrl { get; set; }
        public required string PublicId { get; set; }
        public bool IsMain { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
