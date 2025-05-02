using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.Interfaces.IServices
{
    public interface IPaymentService
    {
        Task CreateOrUpdatePaymentIntentAsync(Guid userId);
    }
}
