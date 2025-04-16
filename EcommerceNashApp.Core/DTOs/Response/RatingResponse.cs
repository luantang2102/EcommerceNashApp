using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.DTOs.Response
{
    public class RatingResponse
    {
        public Guid Id { get; set; }
        public int Value { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public UserResponse User { get; set; } = null!;
        public Guid ProductId { get; set; } = Guid.Empty;
    }
}
