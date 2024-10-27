using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace T2305M_API.Entities
{
    public class CheckBook
    {
        [Key]
        public int CheckBookId { get; set; }// auto-increment

        [Required]
        [ForeignKey("Account")]
        public int AccountId { get; set; } // References the account associated with this checkbook
        public virtual Account? Account { get; set; } // Navigation property to Account
        public int UserId { get; set; } // References the account associated with this checkbook
        public virtual User? User { get; set; } // Navigation property to Account

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.Now; // Date the checkbook was requested 

        [Required]
        public int? TotalChecks { get; set; } // Total number of checks in the checkbook (e.g., 50, 100)

        public int? ChecksRemaining { get; set; } // Tracks how many checks are still available

        public string Status { get; set; } = "PENDING"; // Status of the checkbook (e.g., "Pending", "Approved", "Dispatched", "Delivered", "Cancelled", "LOCKED", "STOPPED")
        public DateTime? ApprovedDate { get; set; } // Date the checkbook request was approved
        public DateTime? DispatchedDate { get; set; } // Date the checkbook was dispatched
        public DateTime? DeliveredDate { get; set; } // Date the checkbook was delivered to the customer

        // Collection of checks that belong to this checkbook
        public virtual ICollection<Check>? Checks { get; set; }
    }

}
