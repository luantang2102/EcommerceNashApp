using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.Paginations;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;

        public RatingService(IRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }

        public async Task<PagedList<RatingResponse>> GetRatingsAsync(RatingParams ratingParams)
        {
            var query = _ratingRepository.GetAllAsync()
                .Filter(ratingParams.Value, ratingParams.HasComment);

            var projectedQuery = query.Select(x => x.MapModelToReponse());

            return await PagedList<RatingResponse>.ToPagedList(
                projectedQuery,
                ratingParams.PageNumber,
                ratingParams.PageSize
            );
        }

        public async Task<RatingResponse> GetRatingByIdAsync(Guid ratingId)
        {
            var rating = await _ratingRepository.GetByIdAsync(ratingId);
            if (rating == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "ratingId", ratingId }
                };
                throw new AppException(ErrorCode.RATING_NOT_FOUND, attributes);
            }

            return rating.MapModelToReponse();
        }

        public async Task<PagedList<RatingResponse>> GetRatingsByProductIdAsync(RatingParams ratingParams, Guid productId)
        {
            var query = _ratingRepository.GetByProductIdAsync(productId)
                .Filter(ratingParams.Value, ratingParams.HasComment);

            var projectedQuery = query.Select(x => x.MapModelToReponse());

            return await PagedList<RatingResponse>.ToPagedList(
                projectedQuery,
                ratingParams.PageNumber,
                ratingParams.PageSize
            );
        }

        public async Task<RatingResponse> CreateRatingAsync(Guid userId, RatingRequest ratingRequest)
        {
            var user = await _ratingRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "userId", userId }
                };
                throw new AppException(ErrorCode.USER_NOT_FOUND, attributes);
            }

            var existingRating = await _ratingRepository.GetByUserAndProductAsync(userId, ratingRequest.ProductId);
            if (existingRating != null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "productId", ratingRequest.ProductId }
                };
                throw new AppException(ErrorCode.RATING_ALREADY_EXISTS, attributes);
            }

            var rating = new Rating
            {
                Value = ratingRequest.Value,
                Comment = ratingRequest.Comment,
                User = user,
                ProductId = ratingRequest.ProductId
            };

            var createdRating = await _ratingRepository.CreateAsync(rating);
            return createdRating.MapModelToReponse();
        }

        public async Task<RatingResponse> UpdateRatingAsync(Guid userId, Guid ratingId, RatingRequest ratingRequest)
        {
            var rating = await _ratingRepository.GetByIdAsync(ratingId);
            if (rating == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "ratingId", ratingId }
                };
                throw new AppException(ErrorCode.RATING_NOT_FOUND, attributes);
            }

            if (rating.UserId != userId)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "ratingId", ratingId }
                };
                throw new AppException(ErrorCode.ACCESS_DENIED, attributes);
            }

            rating.Value = ratingRequest.Value;
            rating.Comment = ratingRequest.Comment;

            await _ratingRepository.UpdateAsync(rating);
            return rating.MapModelToReponse();
        }

        public async Task DeleteRatingAsync(Guid userId, Guid ratingId)
        {
            var rating = await _ratingRepository.GetByIdAsync(ratingId);
            if (rating == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "ratingId", ratingId }
                };
                throw new AppException(ErrorCode.RATING_NOT_FOUND, attributes);
            }

            if (rating.UserId != userId)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "ratingId", ratingId }
                };
                throw new AppException(ErrorCode.ACCESS_DENIED, attributes);
            }

            await _ratingRepository.DeleteAsync(rating);
        }
    }
}