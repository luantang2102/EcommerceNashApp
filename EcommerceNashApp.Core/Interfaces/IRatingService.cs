using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Infrastructure.Helpers.Params;

namespace EcommerceNashApp.Core.Interfaces
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