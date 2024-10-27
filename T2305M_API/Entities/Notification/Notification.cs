namespace T2305M_API.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }// auto-increment
        public string Content { get; set; }
        public bool IsRead { get; set; } = false; // Default to unread
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default to the current time
        public int UserId { get; set; } // Reference to the Id filed of the User (not the UserId)
        public virtual User? User { get; set; } // Reference to the user
    }

}
