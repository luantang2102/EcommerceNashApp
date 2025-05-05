using EcommerceNashApp.Web.Models.Views;

namespace EcommerceNashApp.Web.Services
{
    public interface IRatingService
    {
        Task<List<ProductRatingView>> GetProductRatingsAsync(Guid productId, CancellationToken cancellationToken);
        Task<ProductRatingView?> GetUserRatingAsync(Guid productId, Guid userId, CancellationToken cancellationToken);
        Task<ProductRatingView> CreateRatingAsync(Guid userId, Guid productId, int value, string comment, CancellationToken cancellationToken);
        Task<ProductRatingView> UpdateRatingAsync(Guid userId, Guid productId, Guid ratingId, int value, string comment, CancellationToken cancellationToken);
        Task DeleteRatingAsync(Guid userId, Guid ratingId, CancellationToken cancellationToken);
    }
}
