using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using Stripe;
using System.Text;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserProfileRepository _userProfileRepository;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            IUserProfileRepository userProfileRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _userProfileRepository = userProfileRepository;
        }

        public async Task CreateOrderAsync(Guid userId, bool saveAddress, ShippingAddressRequest shippingAddress)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null || !cart.CartItems.Any())
            {
                var attributes = new Dictionary<string, object>
                {
                    { "userId", userId }
                };
                throw new AppException(ErrorCode.CART_NOT_FOUND, attributes);
            }

            // Verify PaymentIntent status
            if (string.IsNullOrEmpty(cart.PaymentIntentId))
            {
                var attributes = new Dictionary<string, object>
                {
                    { "userId", userId }
                };
                throw new AppException(ErrorCode.INVALID_PAYMENT_INTENT, attributes);
            }

            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(cart.PaymentIntentId);
            if (paymentIntent.Status != "succeeded")
            {
                var attributes = new Dictionary<string, object>
                {
                    { "paymentIntentId", cart.PaymentIntentId },
                    { "status", paymentIntent.Status }
                };
                throw new AppException(ErrorCode.PAYMENT_FAILED, attributes);
            }

            double totalAmount = 0;
            foreach (var item in cart.CartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    var attributes = new Dictionary<string, object>
                    {
                        { "productId", item.ProductId }
                    };
                    throw new AppException(ErrorCode.PRODUCT_NOT_FOUND, attributes);
                }

                if (!product.InStock || product.StockQuantity < item.Quantity)
                {
                    var attributes = new Dictionary<string, object>
                    {
                        { "productId", item.ProductId },
                        { "stockQuantity", product.StockQuantity }
                    };
                    throw new AppException(ErrorCode.INSUFFICIENT_STOCK, attributes);
                }

                totalAmount += item.Price * item.Quantity;
                product.StockQuantity -= item.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            // Concatenate shipping address fields
            var addressParts = new List<string>
            {
                shippingAddress.FullName,
                shippingAddress.Address1,
                shippingAddress.Address2,
                shippingAddress.City,
                shippingAddress.State,
                shippingAddress.Zip,
                shippingAddress.Country
            }.Where(part => !string.IsNullOrEmpty(part));
            var concatenatedAddress = string.Join(", ", addressParts);

            var order = new Order
            {
                UserProfileId = userId,
                TotalAmount = totalAmount + 50000, // Add delivery fee
                Status = "Confirmed",
                OrderDate = DateTime.UtcNow,
                ShippingAddress = concatenatedAddress,
                PaymentIntentId = cart.PaymentIntentId,
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Price
                }).ToList()
            };

            if (saveAddress)
            {
                var userProfile = await _userProfileRepository.GetByIdAsync(userId);
                if (userProfile != null)
                {
                    userProfile.Address = concatenatedAddress;
                    await _userProfileRepository.UpdateAsync(userProfile);
                }
            }

            await _orderRepository.CreateAsync(order);

            // Clear the cart
            await ClearCartAsync(cart);
        }

        public async Task<OrderResponse> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "orderId", orderId }
                };
                throw new AppException(ErrorCode.ORDER_NOT_FOUND, attributes);
            }
            return MapToOrderResponse(order);
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, string status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "orderId", orderId }
                };
                throw new AppException(ErrorCode.ORDER_NOT_FOUND, attributes);
            }

            order.Status = status;
            await _orderRepository.UpdateAsync(order);
        }

        private async Task ClearCartAsync(Cart cart)
        {
            foreach (var item in cart.CartItems.ToList())
            {
                await _cartItemRepository.DeleteAsync(item);
            }
            cart.PaymentIntentId = null;
            cart.ClientSecret = null;
            await _cartRepository.UpdateAsync(cart);
        }

        private OrderResponse MapToOrderResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                UserProfileId = order.UserProfileId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = "Card", // Assuming card payment via Stripe
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponse
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "Unknown",
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };
        }
    }
}