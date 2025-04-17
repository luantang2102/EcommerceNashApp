using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Core.Helpers.Params;

namespace EcommerceNashApp.Core.Interfaces
{
    public interface IRatingService
    {
        Task<PagedList<RatingResponse>> GetRatingsAsync(RatingParams ratingParams);
        Task<RatingResponse> GetRatingByIdAsync(Guid ratingId);
        Task<PagedList<RatingResponse>> GetRatingsByProductIdAsync(RatingParams ratingParams, Guid productId);
        Task<RatingResponse> CreateRatingAsync(RatingRequest ratingRequest, Guid userId);
        Task<RatingResponse> UpdateRatingAsync(Guid ratingId, RatingRequest ratingRequest);
        Task DeleteRatingAsync(Guid ratingId);
    }
}
