using IdentityService.Application.Common.Models;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Features.Users.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public sealed class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        //[HttpGet]
        //[ProducesResponseType(typeof(ApiResult<IReadOnlyList<UserResponse>>), StatusCodes.Status200OK)]
        //public async Task<ActionResult<ApiResult<IReadOnlyList<UserResponse>>>> GetAll(
        //    CancellationToken cancellationToken)
        //{
        //    var users = await _userService.GetAllAsync(cancellationToken);
        //    return Ok(ApiResult<IReadOnlyList<UserResponse>>.Ok(users, "ดึงข้อมูลผู้ใช้งานสำเร็จ"));
        //}


        [HttpGet]
        [ProducesResponseType(typeof(ApiResult<PagedResult<UserResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResult<PagedResult<UserResponse>>>> GetAll(
            [FromQuery] UserQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResult<PagedResult<UserResponse>>.Ok(result, "ดึงข้อมูลผู้ใช้งานสำเร็จ"));
        }


        [HttpGet("{userId:guid}")]
        [ProducesResponseType(typeof(ApiResult<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResult<UserResponse>>> GetById(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(userId, cancellationToken);
            return Ok(ApiResult<UserResponse>.Ok(user, "ดึงข้อมูลผู้ใช้งานสำเร็จ"));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResult<UserResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResult<UserResponse>>> Create(
            [FromBody] CreateUserRequest request,
            CancellationToken cancellationToken)
        {
            var user = await _userService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { userId = user.UserId },
                ApiResult<UserResponse>.Ok(user,"สร้างผู้ใช้งานสำเร็จ"));
        }

        [HttpPut("{userId:guid}")]
        [ProducesResponseType(typeof(ApiResult<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResult<UserResponse>>> Update(
            Guid userId,
            [FromBody] UpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            var user = await _userService.UpdateAsync(userId, request, cancellationToken);
            return Ok(ApiResult<UserResponse>.Ok(user, "แก้ไขข้อมูลผู้ใช้งานสำเร็จ"));
        }

        [HttpDelete("{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            Guid userId,
            CancellationToken cancellationToken)
        {
            await _userService.DeleteAsync(userId, cancellationToken);
            return NoContent();
        }
    }
}
