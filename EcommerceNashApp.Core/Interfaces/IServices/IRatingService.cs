using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.Paginations;

namespace EcommerceNashApp.Core.Interfaces.IServices
{
    public interface IRatingService
    {
        Task<PagedList<RatingResponse>> GetRatingsAsync(RatingParams ratingParams);
        Task<RatingResponse> GetRatingByIdAsync(Guid ratingId);
        Task<PagedList<RatingResponse>> GetRatingsByProductIdAsync(RatingParams ratingParams, Guid productId);
        Task<RatingResponse> CreateRatingAsync(Guid userId, RatingRequest ratingRequest);
        Task<RatingResponse> UpdateRatingAsync(Guid userId, Guid ratingId, RatingRequest ratingRequest);
        Task DeleteRatingAsync(Guid userId, Guid ratingId);
    }
}