using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.DTOs.Request
{
    public class RatingRequest
    {
        public int Value { get; set; }
        public string? Comment { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
    }
}
