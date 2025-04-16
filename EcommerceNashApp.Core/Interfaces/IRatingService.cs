using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Core.Helpers.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.Interfaces
{
    public interface IRatingService
    {
        Task<PagedList<RatingResponse>> GetRatingsAsync(RatingParams ratingParams);
        Task<RatingResponse> GetRatingByIdAsync(Guid ratingId);
        Task<RatingResponse> GetRatingsByProductIdAsync(Guid productId);
        Task<RatingResponse> CreateRatingAsync(RatingRequest ratingRequest, Guid userId);
        Task<RatingResponse> UpdateRatingAsync(Guid ratingId, RatingRequest ratingRequest);
        Task DeleteRatingAsync(Guid ratingId);

    }
}
