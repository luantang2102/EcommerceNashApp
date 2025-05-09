﻿using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Data.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceNashApp.Infrastructure.Data.Configuration
{
    public class ProductConfiguration : BaseEntityConfiguration<Product>
    {
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            base.Configure(builder);

            builder.HasMany(p => p.Categories)
                   .WithMany(c => c.Products)
                   .UsingEntity(j => j.ToTable("ProductCategories"));
        }
    }
}
