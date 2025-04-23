using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.DTOs.Response
{
    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Address { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }

    }
}
