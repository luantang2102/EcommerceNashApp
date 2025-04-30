using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces.IServices;
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
        public async Task<IActionResult> GetRatingsByProductId(Guid productId, [FromQuery] RatingParams ratingParams)
        {
            var ratings = await _ratingService.GetRatingsByProductIdAsync(ratingParams, productId);
            Response.AddPaginationHeader(ratings.Metadata);
            return Ok(new ApiResponse<IEnumerable<RatingResponse>>(200, "Ratings retrieved successfully", ratings));
        }

        [HttpPost]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> CreateRating([FromBody] RatingRequest ratingRequest)
        {
            var userId = User.GetUserId();
            var createdRating = await _ratingService.CreateRatingAsync(userId, ratingRequest);
            return CreatedAtAction(nameof(GetRatingById), new { id = createdRating.Id },
                new ApiResponse<RatingResponse>(201, "Rating created successfully", createdRating));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> UpdateRating(Guid id, [FromBody] RatingRequest ratingRequest)
        {
            var userId = User.GetUserId();
            var updatedRating = await _ratingService.UpdateRatingAsync(userId, id, ratingRequest);
            return Ok(new ApiResponse<RatingResponse>(200, "Rating updated successfully", updatedRating));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> DeleteRating(Guid id)
        {
            var userId = User.GetUserId();
            await _ratingService.DeleteRatingAsync(userId, id);
            return Ok(new ApiResponse<string>(200, "Rating deleted successfully", "Deleted"));
        }
    }
}