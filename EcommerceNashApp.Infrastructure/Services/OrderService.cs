using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Repositories;
using Stripe;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
        }

        public async Task<OrderResponse> CreateOrderAsync(Guid userProfileId, string shippingAddress, string paymentMethod, string paymentIntentId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userProfileId);
            if (cart == null || !cart.CartItems.Any())
            {
                var attributes = new Dictionary<string, object>
                {
                    { "userId", userProfileId }
                };
                throw new AppException(ErrorCode.CART_EMPTY, attributes);
            }

            if (cart.PaymentIntentId != paymentIntentId)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "paymentIntentId", paymentIntentId }
                };
                throw new AppException(ErrorCode.INVALID_PAYMENT_INTENT, attributes);
            }

            // Confirm PaymentIntent
            var service = new PaymentIntentService();
            var paymentIntent = await service.ConfirmAsync(paymentIntentId, new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethod
            });

            if (paymentIntent.Status != "succeeded")
            {
                var attributes = new Dictionary<string, object>
                {
                    { "paymentIntentId", paymentIntentId },
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

            var order = new Order
            {
                UserProfileId = userProfileId,
                TotalAmount = totalAmount + 50000, // Add delivery fee
                Status = "Confirmed",
                OrderDate = DateTime.UtcNow,
                ShippingAddress = shippingAddress,
                PaymentMethod = paymentMethod,
                PaymentIntentId = paymentIntentId,
                OrderItems = new List<OrderItem>()
            };

            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Price,
                    Order = order
                };
                order.OrderItems.Add(orderItem);
                await _orderItemRepository.CreateAsync(orderItem);
            }

            await _orderRepository.CreateAsync(order);

            // Clear the cart after order creation
            await ClearCartAsync(cart);

            return MapToOrderResponse(order);
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
                PaymentMethod = order.PaymentMethod,
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponse
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };
        }
    }
}