using System.ComponentModel.DataAnnotations;

namespace T2305M_API.DTO.Notification
{
    public class GetBasicNotificationDTO
    {
        public int NotificationId { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; } = false; // Default to unread
        public DateTime CreatedAt { get; set; } // Default to the current time
    }

    public class CreateBasicNotificationDTO
    {
        public int UserId { get; set; }
        [Required]
        public string Content { get; set; }
    }
    public class NotificationQueryParameters
    {
        const int maxPageSize = 50;
        public int Page { get; set; } = 1;
        private int _pageSize = 10;

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value > maxPageSize ? maxPageSize : value; }
        }

        public int? UserId { get; set; }
        public bool? IsRead { get; set; }

        // New properties for sorting
        public string? SortColumn { get; set; } = "CreatedAt"; // Default sorting column
        public string? SortOrder { get; set; } = "desc"; // Default sorting order
    }

}
