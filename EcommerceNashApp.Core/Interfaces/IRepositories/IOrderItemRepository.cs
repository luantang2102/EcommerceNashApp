using EcommerceNashApp.Core.Models.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.Interfaces.IRepositories
{
    public interface IOrderItemRepository
    {
        IQueryable<OrderItem> GetAllAsync();
        Task<OrderItem?> GetByIdAsync(Guid orderItemId);
        Task<OrderItem> CreateAsync(OrderItem orderItem);
        Task UpdateAsync(OrderItem orderItem);
        Task DeleteAsync(OrderItem orderItem);
    }
}

