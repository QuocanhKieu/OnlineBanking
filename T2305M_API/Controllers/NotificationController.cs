using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using T2305M_API.DTO.Notification;
using T2305M_API.Entities;
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
        private readonly T2305mApiContext _context;

        public NotificationController(INotificationRepository notificationRepository, T2305mApiContext context)
        {
            _notificationRepository = notificationRepository;
            _context = context;
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


        [HttpPut("mark-all-notifications-as-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated" });
                }

                int userId = int.Parse(userIdClaim);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Retrieve notifications based on role and target
                if (userRole == "USER")
                {
                    // Mark all notifications for the user as read
                    var userNotifications = _context.Notifications
                        .Where(n => n.UserId == userId && n.Target == "USER" && !n.IsRead);

                    foreach (var notification in userNotifications)
                    {
                        notification.IsRead = true;
                    }
                }
                else if (userRole == "ADMIN")
                {
                    // Mark all notifications for the admin as read
                    var adminNotifications = _context.Notifications
                        .Where(n => n.Target == "ADMIN" && !n.IsRead);

                    foreach (var notification in adminNotifications)
                    {
                        notification.IsRead = true;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Successfully marked all notifications as read"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

    }
}
