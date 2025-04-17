using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Core.Models.Identity;
using EcommerceNashApp.Infrastructure.Data;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class RatingService : IRatingService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public RatingService(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<PagedList<RatingResponse>> GetRatingsAsync(RatingParams ratingParams)
        {
            var query = _context.Ratings
                .Include(x => x.User)
                .Include(x => x.Product)
                .Filter(ratingParams.Value, ratingParams.HasComment)
                .AsQueryable();

            var projectedQuery = query.Select(x => x.MapModelToReponse());

            return await PagedList<RatingResponse>.ToPagedList(
                projectedQuery,
                ratingParams.PageNumber,
                ratingParams.PageSize
            );
        }

        public async Task<RatingResponse> GetRatingByIdAsync(Guid ratingId)
        {
            var rating = await _context.Ratings
                .Include(x => x.User)
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == ratingId);

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
            var query = _context.Ratings
                .Include(x => x.User)
                .Include(x => x.Product)
                .Where(x => x.ProductId == productId)
                .Filter(ratingParams.Value, ratingParams.HasComment)
                .AsQueryable();

            var projectedQuery = query.Select(x => x.MapModelToReponse());

            return await PagedList<RatingResponse>.ToPagedList(
                projectedQuery,
                ratingParams.PageNumber,
                ratingParams.PageSize
            );

        }

        public async Task<RatingResponse> CreateRatingAsync(RatingRequest ratingRequest, Guid userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new AppException(ErrorCode.USER_NOT_FOUND, new Dictionary<string, object> { { "userId", userId } });
            }

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(x => x.ProductId == ratingRequest.ProductId && x.UserId == userId);

            if (existingRating != null)
            {
                throw new AppException(ErrorCode.RATING_ALREADY_EXISTS, new Dictionary<string, object>
                {
                    { "productId", ratingRequest.ProductId },
                    { "userId", userId }
                });
            }

            var rating = new Rating
            {
                Value = ratingRequest.Value,
                Comment = ratingRequest.Comment,
                User = user,
                ProductId = ratingRequest.ProductId
            };

            await _context.Ratings.AddAsync(rating);
            await _context.SaveChangesAsync();

            return rating.MapModelToReponse();
        }

        public async Task<RatingResponse> UpdateRatingAsync(Guid ratingId, RatingRequest ratingRequest)
        {
            var rating = await _context.Ratings
                .Include(x => x.User)
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == ratingId);

            if (rating == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "ratingId", ratingId }
                };
                throw new AppException(ErrorCode.RATING_NOT_FOUND, attributes);
            }

            rating.Value = ratingRequest.Value;
            rating.Comment = ratingRequest.Comment;

            _context.Ratings.Update(rating);
            await _context.SaveChangesAsync();

            return rating.MapModelToReponse();
        }

        public async Task DeleteRatingAsync(Guid ratingId)
        {
            var rating = await _context.Ratings
                .Include(x => x.User)
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == ratingId);

            if (rating == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "ratingId", ratingId }
                };
                throw new AppException(ErrorCode.RATING_NOT_FOUND, attributes);
            }

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();
        }
    }
}
