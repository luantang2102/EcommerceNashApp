using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;

        public CartService(ICartRepository cartRepository, ICartItemRepository cartItemRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
        }

        public async Task<CartResponse> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CartItems = []
                };
                await _cartRepository.CreateAsync(cart);
            }
            return cart.MapModelToResponse();
        }

        public async Task<CartItemResponse> AddItemToCartAsync(Guid userId, Guid productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "productId", productId }
                };
                throw new AppException(ErrorCode.PRODUCT_NOT_FOUND, attributes);
            }

            if (!product.InStock || product.StockQuantity < quantity)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "productId", productId },
                    { "stockQuantity", product.StockQuantity }
                };
                throw new AppException(ErrorCode.INSUFFICIENT_STOCK, attributes);
            }

            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CartItems = []
                };
                await _cartRepository.CreateAsync(cart);
            }

            var existingItem = cart.CartItems.FirstOrDefault(x => x.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                if (existingItem.Quantity > product.StockQuantity)
                {
                    var attributes = new Dictionary<string, object>
                    {
                        { "productId", productId },
                        { "stockQuantity", product.StockQuantity }
                    };
                    throw new AppException(ErrorCode.INSUFFICIENT_STOCK, attributes);
                }
                await _cartItemRepository.UpdateAsync(existingItem);
                return existingItem.MapModelToResponse();
            }

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity,
                Price = product.Price
            };
            await _cartItemRepository.CreateAsync(cartItem);
            cart.CartItems.Add(cartItem);
            await _cartRepository.UpdateAsync(cart);

            return cartItem.MapModelToResponse();
        }

        public async Task<CartItemResponse> UpdateCartItemAsync(Guid cartItemId, int quantity)
        {
            var cartItem = await _cartItemRepository.GetByIdAsync(cartItemId);
            if (cartItem == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "cartItemId", cartItemId }
                };
                throw new AppException(ErrorCode.CART_ITEM_NOT_FOUND, attributes);
            }

            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "productId", cartItem.ProductId }
                };
                throw new AppException(ErrorCode.PRODUCT_NOT_FOUND, attributes);
            }

            if (!product.InStock || product.StockQuantity < quantity)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "productId", cartItem.ProductId },
                    { "stockQuantity", product.StockQuantity }
                };
                throw new AppException(ErrorCode.INSUFFICIENT_STOCK, attributes);
            }

            cartItem.Quantity = quantity;
            await _cartItemRepository.UpdateAsync(cartItem);
            return cartItem.MapModelToResponse();
        }

        public async Task DeleteCartItemAsync(Guid cartItemId)
        {
            var cartItem = await _cartItemRepository.GetByIdAsync(cartItemId);
            if (cartItem == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "cartItemId", cartItemId }
                };
                throw new AppException(ErrorCode.CART_ITEM_NOT_FOUND, attributes);
            }

            await _cartItemRepository.DeleteAsync(cartItem);
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
            {
                return;
            }

            foreach (var item in cart.CartItems.ToList())
            {
                await _cartItemRepository.DeleteAsync(item);
            }
            cart.PaymentIntentId = null;
            cart.ClientSecret = null;
            await _cartRepository.UpdateAsync(cart);
        }
    }
}