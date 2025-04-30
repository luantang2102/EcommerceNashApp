using EcommerceNashApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceNashApp.Test.Utilities
{
    public class DbContextCollection : ServiceCollection
    {
        public DbContextCollection()
        {
            var optionBuilder = new DbContextOptionsBuilder<AppDbContext>
            {
            }
        }
    }
}
