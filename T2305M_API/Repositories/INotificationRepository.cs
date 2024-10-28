using T2305M_API.DTO.Notification;
using T2305M_API.Entities;
using T2305M_API.Models;

namespace T2305M_API.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification> CreateNotificationAsync(CreateBasicNotificationDTO createBasicNotificationDTO);
        Task<PaginatedResult<GetBasicNotificationDTO>> GetNotificationsAsync(NotificationQueryParameters queryParameters);
        Task MarkAsReadAsync(int notificationId);
    }

}
