using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Infrastructure.Extensions
{
    public static class CategoryExtensions
    {
        public static CategoryResponse MapModelToResponse(this Category category)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Level = category.Level,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate
            };
        }
    }
}
