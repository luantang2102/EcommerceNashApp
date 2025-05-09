﻿using System;

namespace EcommerceNashApp.Shared.DTOs.Request
{
    public class CategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Guid? ParentCategoryId { get; set; } = null;

    }
}
