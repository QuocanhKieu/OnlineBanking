using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using T2305M_API.DTO.Notification;
using T2305M_API.Models;
using T2305M_API.Repositories; // Assuming you have a repository for notifications

namespace T2305M_API.Controllers

{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        // GET: api/Notification
        [HttpGet("get-notifications")]
        public async Task<ActionResult> GetNotifications([FromQuery] NotificationQueryParameters queryParameters)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }


                int userId = int.Parse(userIdClaim);
                queryParameters.UserId = userId;
                var paginatedResult = await _notificationRepository.GetNotificationsAsync(queryParameters);

                return Ok(new
                {
                    result = paginatedResult,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }

        [HttpPut("mark-as-read/{notificationId}")]
        public async Task<ActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                await _notificationRepository.MarkAsReadAsync(notificationId);
                return Ok(new
                {
                    message = "Notification marked as read successfully.",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messsage = "Internal server error: " + ex.Message });
            }
        }
    }
}
