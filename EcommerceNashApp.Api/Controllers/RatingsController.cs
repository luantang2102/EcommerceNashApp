using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNashApp.Api.Controllers
{
    public class RatingsController : BaseApiController
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRatings([FromQuery] RatingParams ratingParams)
        {
            var ratings = await _ratingService.GetRatingsAsync(ratingParams);
            Response.AddPaginationHeader(ratings.Metadata);
            return Ok(new ApiResponse<IEnumerable<RatingResponse>>(200, "Ratings retrieved successfully", ratings));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRatingById(Guid id)
        {
            var rating = await _ratingService.GetRatingByIdAsync(id);
            return Ok(new ApiResponse<RatingResponse>(200, "Rating retrieved successfully", rating));
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetRatingsByProductId(Guid productId)
        {
            var ratings = await _ratingService.GetRatingsByProductIdAsync(productId);
            return Ok(new ApiResponse<RatingResponse>(200, "Ratings retrieved successfully", ratings));
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CreateRating([FromBody] RatingRequest ratingRequest)
        {
            var userId = User.GetUserId();
            var createdRating = await _ratingService.CreateRatingAsync(ratingRequest, userId);
            return Ok(new ApiResponse<RatingResponse>(201, "Rating created successfully", createdRating));
        }

        [Authorize(Roles = "User")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRating(Guid id, [FromBody] RatingRequest ratingRequest)
        {
            var updatedRating = await _ratingService.UpdateRatingAsync(id, ratingRequest);
            return Ok(new ApiResponse<RatingResponse>(200, "Rating updated successfully", updatedRating));
        }

        [Authorize(Roles = "User")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRating(Guid id)
        {
            await _ratingService.DeleteRatingAsync(id);
            return Ok(new ApiResponse<string>(200, "Rating deleted successfully", "Deleted"));
        }
    }
}
