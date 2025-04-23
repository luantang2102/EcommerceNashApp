using CloudinaryDotNet.Actions;
using EcommerceNashApp.Infrastructure.Helpers.Params.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.Helpers.Params
{
    public class UserParams : PaginationParams
    {
        public string? OrderBy { get; set; }
        public string? SearchTerm { get; set; }
    }
}
