﻿using EcommerceNashApp.Shared.Paginations;

namespace EcommerceNashApp.Core.Helpers.Params
{
    public class CategoryParams : PaginationParams
    {
        public string? OrderBy { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string? Level { get; set; }
    }
}
