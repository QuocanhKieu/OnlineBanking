using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Security.Claims;
using T2305M_API.DTO.User;
using T2305M_API.Entities;
using T2305M_API.Models;
using T2305M_API.Services;
using T2305M_API.Services.Implements;
using T2305M_API.Repositories.Implements;
using T2305M_API.Repositories;
using T2305M_API.DTO.User;
using T2305M_API.DTO.User;

namespace T2305M_API.Controllers
{
    [Authorize]

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly T2305mApiContext _context;

        public UserController(
            IUserService userService,
            IUserRepository userRepository,
            ITransactionRepository transactionRepository,
            T2305mApiContext context)
        {
            _userService = userService;
            _userRepository = userRepository;
            _transactionRepository = transactionRepository;
            _context = context;
        }

        //[HttpGet("list-user")]
        //public async Task<IActionResult> ListUserUsers(UserQueryParameters UserQueryParameters)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(new
        //            {
        //                code = 1,
        //                message = "Please input valid user information."
        //            });
        //        }
        //        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (userIdClaim == null)
        //        {
        //            return Unauthorized(new { message = "Invalid token or user not authenticated" });
        //        }

        //        int userId = int.Parse(userIdClaim);
        //        UserQueryParameters.Userid = userId;
        //        var paginatedResult = await _UserService.GetBasicUsersAsync(UserQueryParameters);
        //        return Ok(new
        //        {
        //            result = paginatedResult,
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
        //    }
        //}

        [HttpGet("get-detail-User")]
        public async Task<ActionResult<GetDetailUserDTO>> GetDetailUserDTO()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid user information."
                    });
                }
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);
                var detailUserDTO = await _userService.GetDetailUserDTOAsync(userId);
                if (detailUserDTO == null)
                {
                    return NotFound(
                        new
                        {
                            message = "DetailUser not found."
                        });
                }
                //return Ok(new APIResponse<GetDetailUserDTO>(detailUserDTO, "Retrieved paginated basic Books successfully."));
                return Ok(detailUserDTO); // Return the DTO
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }

        [HttpPost("make-trans-password")]
        public async Task<IActionResult> MakeTransPassword([FromBody] TransPasswordDTO transPasswordDTO)
        {
            try
            {

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid user information."
                    });
                }

                var updatedUser = await _userService.MakeTransPasswordAsync(userId, transPasswordDTO);
                if (updatedUser == null)
                {
                    return BadRequest(new
                    {
                        message = "Creating TransPassword not successful, User not found"
                    });
                }

                //await _accountRepository.CreateAccountAsync(createAccountDTO, userId);
                return Ok(new
                {
                    message = "Creating TransPassword successfully"
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            try
            {

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid user information."
                    });
                }

                var updatedUser = await _userService.UploadAvatarAsync(userId, file);
                if (updatedUser == null)
                {
                    return BadRequest(new
                    {
                        message = "Upload Avatar not successful, User not found"
                    });
                }
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                var imageUrl = $"{baseUrl}{updatedUser.Avatar}";

                //await _accountRepository.CreateAccountAsync(createAccountDTO, userId);
                return Ok(new
                {
                    avatarLInk = imageUrl,
                    message = "Upload Avatar successfully"
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }


        [HttpGet("verify-transpassword")]
        public async Task<ActionResult<GetDetailUserDTO>> VerifyTranspassword([FromQuery ]string transPassword)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid user information."
                    });
                }
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);
                var isMatch = await _userService.VerifyTranspasswordAsync(userId, transPassword);
                if (!isMatch)
                {
                    return BadRequest(
                        new
                        {
                            message = "Transaction password not match, Please Check Again!"
                        });
                }
                return Ok(
                new
                {
                    message = "Transction password matches correctly"
                }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }

    }
}




