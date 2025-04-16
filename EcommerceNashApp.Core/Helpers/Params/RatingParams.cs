using EcommerceNashApp.Infrastructure.Helpers.Params.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.Helpers.Params
{
    public class RatingParams : PaginationParams
    {
        public string? OrderBy { get; set; }
        public string? Value { get; set; }
        public string? HasComment { get; set; }
    }
}
