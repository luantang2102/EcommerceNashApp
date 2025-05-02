using EcommerceNashApp.Core.Models.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.Interfaces.IRepositories
{
    public interface ICartItemRepository
    {
        IQueryable<CartItem> GetAllAsync();
        Task<CartItem?> GetByIdAsync(Guid cartItemId);
        Task<CartItem> CreateAsync(CartItem cartItem);
        Task UpdateAsync(CartItem cartItem);
        Task DeleteAsync(CartItem cartItem);
    }
}
