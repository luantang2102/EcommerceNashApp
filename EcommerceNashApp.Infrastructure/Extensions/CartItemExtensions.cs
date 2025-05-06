using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Shared.DTOs.Response;

namespace EcommerceNashApp.Infrastructure.Extensions
{
    public static class CartItemExtensions
    {
        public static IQueryable<CartItem> Sort(this IQueryable<CartItem> query, string? orderBy)
        {
            query = orderBy switch
            {
                "dateCreatedDesc" => query.OrderByDescending(x => x.CreatedDate),
                _ => query.OrderBy(x => x.CreatedDate),
            };
            return query;
        }

        public static CartItemResponse MapModelToResponse(this CartItem cartItem)
        {
            return new CartItemResponse
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.Name,
                Images = cartItem.Product.ProductImages.Select(pi => pi.MapModelToResponse()).ToList(),
                Quantity = cartItem.Quantity,
                Price = cartItem.Price
            };
        }
    }
}
