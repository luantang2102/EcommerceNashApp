using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Data;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class RatingService
    {
        private readonly AppDbContext _context;

        public RatingService(AppDbContext context)
        {
            _context = context;
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

        public async Task<RatingResponse> CreateRatingAsync(RatingRequest ratingRequest, Guid userId)
        {
            var rating = new Rating
            {
                Value = ratingRequest.Value,
                Comment = ratingRequest.Comment,
                UserId = userId,
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
