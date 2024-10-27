using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Security.Claims;
using T2305M_API.CustomException;
using T2305M_API.DTO.History;
using T2305M_API.DTO.User;
using T2305M_API.Entities;
using T2305M_API.Models;
using T2305M_API.Services;
using T2305M_API.Services.Implements;
using T2305M_API.Repositories.Implements;
using T2305M_API.Repositories;

namespace T2305M_API.Controllers
{
    [Authorize]

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly T2305mApiContext _context;

        public UserController(IUserService userService, IUserRepository userRepository, T2305mApiContext context)
        {
            _userService = userService;
            _userRepository = userRepository;
            _context = context;

        }

        //[HttpGet]
        //public async Task<ActionResult<GetDetailUserDTO>> GetDetailUserDTOById()
        //{
        //    try
        //    {
        //        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (userIdClaim == null)
        //        {
        //            return Unauthorized(new { message = "Invalid token or user not authenticated" });
        //        }


        //        int userId = int.Parse(userIdClaim);

        //        var detailUserDTO = await _userService.GetDetailUserDTOByIdAsync(userId);
        //        if (detailUserDTO == null)
        //        {
        //            return NotFound(new APIResponse<GetDetailUserDTO>(HttpStatusCode.NotFound, "DetailUser not found.")); // Return 404 if not found
        //        }
        //        return Ok(detailUserDTO); // Return the DTO
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new APIResponse<GetDetailUserDTO>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message));
        //    }
        //}

        // PUT: api/User
        //[HttpPut]
        //public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO updateUserDTO)
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (userIdClaim == null)
        //    {
        //        return Unauthorized(new { message = "Invalid token or user not authenticated" });
        //    }


        //    int userId = int.Parse(userIdClaim);

        //    var validationErrors = await _userService.ValidateUpdateUserDTO( updateUserDTO);

        //    if (validationErrors != null)
        //    {
        //        return BadRequest(new APIResponse<Dictionary<string, List<string>>>(
        //            HttpStatusCode.BadRequest, "Validation failed", validationErrors));
        //    }
        //    try
        //    {
        //        var updateUserResponseDTO = await _userService.UpdateUserAsync(userId, updateUserDTO);

        //        if (updateUserResponseDTO.UserId > 0)
        //        {
        //            return Ok(new APIResponse<UpdateUserResponseDTO>(updateUserResponseDTO, updateUserResponseDTO.Message));
        //        }

        //        return StatusCode((int)HttpStatusCode.Conflict, new APIResponse<UpdateUserResponseDTO>(
        //            HttpStatusCode.Conflict, "Can not Update User due to internal problems contact the backend."));
        //    }
        //    catch (UserUpdateException ex)
        //    {
        //        return StatusCode((int)HttpStatusCode.InternalServerError, new APIResponse<UpdateUserResponseDTO>(
        //            HttpStatusCode.InternalServerError, "Internal server error: " + ex.ResponseDTO.Message, ex.ResponseDTO));
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode((int)HttpStatusCode.InternalServerError, new APIResponse<UpdateUserResponseDTO>(
        //            HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message));
        //    }
        //}


        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token or user not authenticated" });
            }

            int userId = int.Parse(userIdClaim);

            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                var response = await _userService.UploadAvatarAsync(userId, file);

                return Ok(response);

            }
            catch (FileNotFoundException ex)
            {
                return StatusCode((int)HttpStatusCode.NotFound, new APIResponse<UpdateAvatarResponseDTO>(
                       HttpStatusCode.NotFound, "Not found error: " + ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new APIResponse<UpdateUserResponseDTO>(
                    HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message));
            }
        }


        [HttpPatch("update-address")]
        public async Task<IActionResult> UpdateAddress([FromBody] UpdateUserAddressDTO updateUserAddressDTO)
        {
            // Verify if the user is authenticated
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token or user not authenticated" });
            }

            int userId = int.Parse(userIdClaim);

            try
            {
                // Fetch the user entity from the database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                user.Address = updateUserAddressDTO.Address;

                // Update the user in the database
                await _context.SaveChangesAsync();


                return Ok(new { message = "Address updated successfully.", address = user.Address });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}




