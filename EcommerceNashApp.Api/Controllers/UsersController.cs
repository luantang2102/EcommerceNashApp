using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNashApp.Api.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            var users = await _userService.GetUsersAsync(userParams);
            Response.AddPaginationHeader(users.Metadata);
            return Ok(new ApiResponse<IEnumerable<UserResponse>>(200, "Users retrieved successfully", users));
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(new ApiResponse<UserResponse>(200, "User retrieved successfully", user));
        }

    }
}
